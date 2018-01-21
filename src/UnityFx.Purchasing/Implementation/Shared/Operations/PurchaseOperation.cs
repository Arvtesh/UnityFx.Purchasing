// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	/// <summary>
	/// A purchase operation.
	/// </summary>
	internal class PurchaseOperation : StoreOperation, IStoreOperation<PurchaseResult>, IPurchaseResult
	{
		#region data

		private readonly string _productId;
		private readonly bool _restored;

		private Product _product;
		private StoreTransaction _transaction;
		private PurchaseValidationResult _validationResult;

		#endregion

		#region interface

		internal PurchaseResult ResultUnsafe => new PurchaseResult(this);

		public PurchaseOperation(StoreOperationContainer parent, string productId, bool restored, AsyncCallback asyncCallback, object asyncState)
			: base(parent, StoreOperationId.Purchase, asyncCallback, asyncState, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert(parent != null);
			Debug.Assert(productId != null);

			_productId = productId;
			_restored = restored;

			Store.InvokePurchaseInitiated(this, productId, restored);
		}

		public PurchaseOperation(StoreOperationContainer parent, Product product, bool restored)
			: this(parent, product.definition.id, restored, null, null)
		{
			_product = product;
			_transaction = new StoreTransaction(product);
		}

		public void Initiate()
		{
			var product = Store.Controller.products.WithID(_productId);

			if (product != null && product.availableToPurchase)
			{
				Console.TraceEvent(TraceEventType.Verbose, (int)StoreOperationId.Purchase, $"InitiatePurchase: {_productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
				Store.Controller.InitiatePurchase(product);
			}
			else
			{
				SetFailed(product, StorePurchaseError.ProductUnavailable);
			}
		}

		public bool ProcessPurchase(Product product)
		{
			// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
			// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
			if (_restored || IsSame(product))
			{
				_product = product;
				_transaction = new StoreTransaction(product);
				return true;
			}

			return false;
		}

		public PurchaseProcessingResult Validate()
		{
			Debug.Assert(_product != null);

			try
			{
				Console.TraceEvent(TraceEventType.Verbose, (int)StoreOperationId.Purchase, $"ValidatePurchase: {_productId}, transactionId = {_product.transactionID}");

				if (string.IsNullOrEmpty(_transaction.Receipt))
				{
					SetFailed(StorePurchaseError.ReceiptNullOrEmpty);
				}
				else
				{
#if UNITYFX_SUPPORT_TAP

					try
					{
						Store.ValidatePurchaseAsync(_transaction).ContinueWith(ValidateContinuation);
					}
					catch (Exception e)
					{
						Console.TraceException(StoreOperationId.Purchase, e);
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
						return PurchaseProcessingResult.Complete;
					}

#else

					var validationImplemented = true;

					try
					{
						validationImplemented = Store.ValidatePurchase(_transaction, ValidateCallback);
					}
					catch (Exception e)
					{
						Console.TraceException(StoreOperationId.Purchase, e);
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
						return PurchaseProcessingResult.Complete;
					}

					if (validationImplemented)
					{
						// Check if the validation callback was called synchronously.
						if (!IsCompleted)
						{
							return PurchaseProcessingResult.Pending;
						}
					}
					else
					{
						SetCompleted(new PurchaseValidationResult(PurchaseValidationStatus.Suppressed));
					}

#endif
				}
			}
			catch (Exception e)
			{
				SetFailed(e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void SetCompleted(PurchaseValidationResult validationResult)
		{
			if (TrySetCompleted())
			{
				Store.InvokePurchaseCompleted(this, StorePurchaseError.None, null);
			}
		}

		public void SetFailed(StorePurchaseError reason, Exception e = null)
		{
			SetFailed(default(PurchaseValidationResult), reason, e);
		}

		public void SetFailed(Exception e, bool completedSynchronously = false)
		{
			Console.TraceException(StoreOperationId.Purchase, e);

			if (TrySetException(e, completedSynchronously))
			{
				if (e is StorePurchaseException spe)
				{
					Store.InvokePurchaseCompleted(this, spe.Reason, e);
				}
				else if (e is StoreFetchException sfe)
				{
					Store.InvokePurchaseCompleted(this, StorePurchaseError.StoreNotInitialized, e);
				}
				else
				{
					Store.InvokePurchaseCompleted(this, StorePurchaseError.Unknown, e);
				}
			}
		}

		public void SetFailed(Product product, StorePurchaseError reason, Exception e = null)
		{
			Console.TraceError(StoreOperationId.Purchase, reason.ToString());

			if (e == null)
			{
				e = new StorePurchaseException(this, reason, e);
			}

			if (reason == StorePurchaseError.UserCanceled)
			{
				if (TrySetCanceled())
				{
					Store.InvokePurchaseCompleted(this, reason, e);
				}
			}
			else if (TrySetException(e))
			{
				Store.InvokePurchaseCompleted(this, reason, e);
			}
		}

		public void SetFailed(PurchaseValidationResult validationResult, StorePurchaseError reason, Exception e = null)
		{
			Console.TraceError(StoreOperationId.Purchase, reason.ToString());

			if (e == null)
			{
				e = new StorePurchaseException(this, reason, e);
			}

			if (TrySetException(e))
			{
				Store.InvokePurchaseCompleted(this, reason, e);
			}
		}

		public bool IsSame(Product product)
		{
			return product != null && product.definition.id == _productId;
		}

		#endregion

		#region IPurchaseResult

		/// <inheritdoc/>
		public string ProductId => _productId;

		/// <inheritdoc/>
		public Product Product => _product;

		/// <inheritdoc/>
		public StoreTransaction Transaction => _transaction;

		/// <inheritdoc/>
		public PurchaseValidationResult ValidationResult => _validationResult;

		/// <inheritdoc/>
		public bool Restored => _restored;

		#endregion

		#region IStoreOperation

		/// <inheritdoc/>
		public PurchaseResult Result
		{
			get
			{
				ThrowIfDisposed();
				ThrowIfNotCompletedSuccessfully();
				return new PurchaseResult(this);
			}
		}

		#endregion

		#region implementation

		private void ProcessValidationResult()
		{
			Debug.Assert(_transaction != null);

			try
			{
				if (_validationResult == null)
				{
					_validationResult = new PurchaseValidationResult(PurchaseValidationStatus.Suppressed);
				}

				var status = _validationResult.Status;
				var product = _transaction.Product;

				if (status == PurchaseValidationStatus.NotAvailable)
				{
					// Need to re-validate the purchase: do not confirm.
					SetFailed(_validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
				}
				else
				{
					Console.TraceEvent(TraceEventType.Verbose, (int)StoreOperationId.Purchase, "ConfirmPendingPurchase: " + product.definition.id);
					Store.Controller.ConfirmPendingPurchase(product);

					if (status == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed.
						SetFailed(_validationResult, StorePurchaseError.ReceiptValidationFailed);
					}
					else
					{
						// The purchase validation succeeded.
						SetCompleted(_validationResult);
					}
				}
			}
			catch (Exception e)
			{
				Console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
				TrySetException(e);
			}
		}

#if UNITYFX_SUPPORT_TAP

		private void ValidateContinuation(Task<PurchaseValidationResult> task)
		{
			if (!IsCompleted)
			{
				if (task.Status == TaskStatus.RanToCompletion)
				{
					_validationResult = task.Result;
					ProcessValidationResult();
				}
				else
				{
					SetFailed(StorePurchaseError.ReceiptValidationNotAvailable, task.Exception?.InnerException);
				}
			}
		}

#else

		private void ValidateCallback(PurchaseValidationResult validationResult)
		{
			if (!IsCompleted)
			{
				_validationResult = validationResult;
				ProcessValidationResult();
			}
		}

#endif

		#endregion
	}
}
