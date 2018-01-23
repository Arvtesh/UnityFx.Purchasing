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
	internal class PurchaseOperation : StoreOperation, IStoreOperation<PurchaseResult>, IPurchaseResult, IStoreTransaction
	{
		#region data

		private readonly string _productId;
		private readonly bool _restored;

		private Product _product;
		private string _receipt;
		private PurchaseValidationResult _validationResult;

		#endregion

		#region interface

		internal PurchaseResult ResultUnsafe => new PurchaseResult(this);

		public PurchaseOperation(StoreOperationContainer parent, StoreOperationType opType, string productId, bool restored, AsyncCallback asyncCallback, object asyncState)
			: base(parent, StoreOperationType.Purchase, asyncCallback, asyncState, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert((opType & StoreOperationType.Purchase) != 0);
			Debug.Assert(parent != null);
			Debug.Assert(productId != null);

			_productId = productId;
			_restored = restored;

			Store.InvokePurchaseInitiated(this, productId, restored);
		}

		public PurchaseOperation(StoreOperationContainer parent, Product product, bool restored)
			: this(parent, StoreOperationType.Purchase, product.definition.id, restored, null, null)
		{
			_product = product;
			_receipt = product.GetNativeReceipt();
		}

		public void Initiate()
		{
			var product = Store.Controller.products.WithID(_productId);

			if (product != null && product.availableToPurchase)
			{
				Console.TraceEvent(TraceEventType.Verbose, (int)StoreOperationType.Purchase, $"InitiatePurchase: {_productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
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
				_receipt = product.GetNativeReceipt();
				return true;
			}

			return false;
		}

		public PurchaseProcessingResult Validate()
		{
			Debug.Assert(_product != null);

			try
			{
				TraceEvent(TraceEventType.Verbose, $"ValidatePurchase: {_productId}, transactionId = {_product.transactionID}");

				if (string.IsNullOrEmpty(_receipt))
				{
					SetFailed(StorePurchaseError.ReceiptNullOrEmpty);
				}
				else
				{
#if UNITYFX_SUPPORT_TAP

					try
					{
						Store.ValidatePurchaseAsync(this).ContinueWith(ValidateContinuation);
					}
					catch (Exception e)
					{
						TraceException(e);
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
						return PurchaseProcessingResult.Complete;
					}

#else

					var validationImplemented = true;

					try
					{
						validationImplemented = Store.ValidatePurchase(this, ValidateCallback);
					}
					catch (Exception e)
					{
						TraceException(e);
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
						SetCompleted(PurchaseValidationResult.Suppressed);
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
			TraceException(e);

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
			TraceError(reason.ToString());

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
			TraceError(reason.ToString());

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
		public string TransactionId => _product?.transactionID;

		/// <inheritdoc/>
		public string Receipt => _receipt;

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
			Debug.Assert(_product != null);

			try
			{
				if (_validationResult == null)
				{
					_validationResult = PurchaseValidationResult.Suppressed;
				}

				var status = _validationResult.Status;
				var product = _product;

				if (status == PurchaseValidationStatus.NotAvailable)
				{
					// Need to re-validate the purchase: do not confirm.
					SetFailed(_validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
				}
				else
				{
					TraceEvent(TraceEventType.Verbose, "ConfirmPendingPurchase: " + product.definition.id);
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
				TraceException(e);
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
