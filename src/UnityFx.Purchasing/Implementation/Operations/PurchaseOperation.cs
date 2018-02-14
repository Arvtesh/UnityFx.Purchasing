// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	/// <summary>
	/// A purchase operation.
	/// </summary>
	internal class PurchaseOperation : StoreOperation, IAsyncOperation<PurchaseResult>, IPurchaseResult
	{
		#region data

		private class CompletionSource : AsyncCompletionSource<PurchaseValidationResult>
		{
			private readonly PurchaseOperation _op;
			private bool _completed;

			public CompletionSource(PurchaseOperation op)
			{
				_op = op;
			}

			public override bool TrySetCanceled()
			{
				if (!_completed)
				{
					_completed = true;
					_op.SetValidationException(null);
					return true;
				}

				return false;
			}

			public override bool TrySetException(Exception e)
			{
				if (!_completed)
				{
					_completed = true;
					_op.SetValidationException(e);
					return true;
				}

				return false;
			}

			public override bool TrySetResult(PurchaseValidationResult result)
			{
				if (!_completed)
				{
					_completed = true;
					_op.SetValidationResult(result);
					return true;
				}

				return false;
			}
		}

		private readonly string _productId;
		private readonly bool _restored;

		private Product _product;
		private string _receipt;
		private PurchaseValidationResult _validationResult;
		private int _threadId;

		#endregion

		#region interface

		internal PurchaseResult ResultUnsafe => new PurchaseResult(this);

		public PurchaseOperation(StoreService store, string productId, bool restored, AsyncCallback asyncCallback, object asyncState)
			: base(store, StoreOperationType.Purchase, asyncCallback, asyncState, GetComment(productId, restored))
		{
			Debug.Assert(store != null);
			Debug.Assert(productId != null);

			_productId = productId;
			_restored = restored;

			Store.OnPurchaseInitiated(this, productId, restored);
		}

		public PurchaseOperation(StoreService store, Product product, bool restored)
			: this(store, product.definition.id, restored, null, null)
		{
			_product = product;
			_receipt = product.GetNativeReceipt();
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
					_threadId = Thread.CurrentThread.ManagedThreadId;

					try
					{
						Store.ValidatePurchase(this, new CompletionSource(this));
					}
					catch (Exception e)
					{
						TraceException(e);
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
						return PurchaseProcessingResult.Complete;
					}

					// Check if the validation callback was called synchronously.
					if (!IsCompleted)
					{
						return PurchaseProcessingResult.Pending;
					}
				}
			}
			catch (Exception e)
			{
				SetFailed(e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void SetFailed(StorePurchaseError reason, Exception e = null)
		{
			TraceError(reason.ToString());

			if (e == null)
			{
				e = new StorePurchaseException(this, reason, e);
			}

			if (TrySetException(e, false))
			{
				Store.OnPurchaseCompleted(this, reason, e);
			}
		}

		public void SetFailed(Exception e, bool completedSynchronously = false)
		{
			TraceException(e);

			if (TrySetException(e, completedSynchronously))
			{
				if (e is StorePurchaseException spe)
				{
					Store.OnPurchaseCompleted(this, spe.Reason, e);
				}
				else if (e is StoreFetchException sfe)
				{
					Store.OnPurchaseCompleted(this, StorePurchaseError.StoreNotInitialized, e);
				}
				else
				{
					Store.OnPurchaseCompleted(this, StorePurchaseError.Unknown, e);
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
				if (TrySetCanceled(false))
				{
					Store.OnPurchaseCompleted(this, reason, e);
				}
			}
			else if (TrySetException(e, false))
			{
				Store.OnPurchaseCompleted(this, reason, e);
			}
		}

		public bool IsSame(Product product)
		{
			return product != null && product.definition.id == _productId;
		}

		#endregion

		#region AsyncResult

		protected override void OnStatusChanged(AsyncOperationStatus status)
		{
			base.OnStatusChanged(status);

			if (status == AsyncOperationStatus.Running && !_restored)
			{
				var product = Store.Controller.products.WithID(_productId);

				if (product != null && product.availableToPurchase)
				{
					TraceEvent(TraceEventType.Verbose, $"InitiatePurchase: {_productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
					Store.Controller.InitiatePurchase(product);
				}
				else
				{
					SetFailed(product, StorePurchaseError.ProductUnavailable);
				}
			}
		}

		#endregion

		#region IPurchaseResult

		/// <inheritdoc/>
		public PurchaseValidationResult ValidationResult => _validationResult;

		/// <inheritdoc/>
		public bool Restored => _restored;

		#endregion

		#region IStoreTransaction

		/// <inheritdoc/>
		public string ProductId => _productId;

		/// <inheritdoc/>
		public Product Product => _product;

		/// <inheritdoc/>
		public string TransactionId => _product?.transactionID;

		/// <inheritdoc/>
		public string Receipt => _receipt;

		#endregion

		#region IStoreOperation

		/// <inheritdoc/>
		public PurchaseResult Result
		{
			get
			{
				if (!IsCompletedSuccessfully)
				{
					throw new InvalidOperationException("The operation result is not available.");
				}

				return new PurchaseResult(this);
			}
		}

		#endregion

		#region implementation

		private void ProcessValidationResult(bool calledSynchronously)
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
					SetFailed(StorePurchaseError.ReceiptValidationNotAvailable);
				}
				else
				{
					if (!calledSynchronously)
					{
						TraceEvent(TraceEventType.Verbose, "ConfirmPendingPurchase: " + product.definition.id);
						Store.Controller.ConfirmPendingPurchase(product);
					}

					if (status == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed.
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
					}
					else
					{
						// The purchase validation succeeded.
						if (TrySetCompleted(false))
						{
							Store.OnPurchaseCompleted(this, StorePurchaseError.None, null);
						}
					}
				}
			}
			catch (Exception e)
			{
				TraceException(e);
				TrySetException(e, false);
			}
		}

		private void SetValidationResult(PurchaseValidationResult validationResult)
		{
			_validationResult = validationResult;

			if (_threadId == Thread.CurrentThread.ManagedThreadId)
			{
				ProcessValidationResult(true);
			}
			else
			{
				Store.QueueOnMainThread(
					args =>
					{
						if (!IsCompleted)
						{
							ProcessValidationResult(false);
						}
					},
					validationResult);
			}
		}

		private void SetValidationException(Exception e)
		{
			if (_threadId == Thread.CurrentThread.ManagedThreadId)
			{
				SetFailed(StorePurchaseError.ReceiptValidationFailed, e);
			}
			else
			{
				Store.QueueOnMainThread(
					args =>
					{
						if (!IsCompleted)
						{
							SetFailed(StorePurchaseError.ReceiptValidationFailed, args as Exception);
						}
					},
					e);
			}
		}

		private static string GetComment(string productId, bool restored)
		{
			var result = productId;

			if (restored)
			{
				result += ", auto-restored";
			}

			return result;
		}

		#endregion
	}
}
