// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class StoreService : IStoreListener
	{
		#region data
		#endregion

		#region IStoreListener

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventInitialize, "OnInitialized");
				_storeController = controller;
				_initializeOpCs.SetResult(null);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Error, _traceEventInitialize, "OnInitializeFailed: " + error);
				_initializeOpCs.SetException(new StoreInitializeException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			try
			{
				var product = args.purchasedProduct;
				var productId = product.definition.id;

				// If the purchase operation has been auto-restored, _purchaseOpCs would be null.
				if (_purchaseOpCs == null)
				{
					_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "Purchase: " + productId);
				}

				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, "ProcessPurchase: " + productId);
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

				// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
				// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
				if (_purchaseOpCs == null || _purchaseOpCs.Task.AsyncState.Equals(product))
				{
					var nativeReceipt = ProductExtensions.GetNativeReceipt(product, out string storeId);

					if (string.IsNullOrEmpty(nativeReceipt))
					{
						SetPurchaseFailed(product, StorePurchaseError.ReceiptNullOrEmpty, storeId);
					}
					else
					{
						ValidatePurchase(product, storeId, nativeReceipt);
						return PurchaseProcessingResult.Pending;
					}
				}
			}
			catch (Exception e)
			{
				SetPurchaseFailed(args.purchasedProduct, StorePurchaseError.Unknown, null, e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var productId = product.definition.id;

			// If the purchase operation has been auto-restored, _purchaseOpCs would be null.
			if (_purchaseOpCs == null)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "Purchase: " + productId);
			}

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"OnPurchaseFailed: {productId}, reason={failReason}");

			SetPurchaseFailed(product, GetPurchaseError(failReason), null);
		}

		#endregion

		#region implementation

		private async void ValidatePurchase(Product product, string storeId, string nativeReceipt)
		{
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId={product.transactionID}");

				var userProduct = _products[product.definition.id];
				var transactionInfo = new StoreTransaction(userProduct, product.transactionID, storeId, nativeReceipt, _purchaseOpCs == null);
				var validationResult = await _delegate.ValidatePurchaseAsync(transactionInfo);

				if (validationResult == null)
				{
					// No result returned from the validator means validation succeeded.
					ConfirmPendingPurchase(product);
					SetPurchaseCompleted(product, transactionInfo, validationResult);
				}
				else
				{
					resultStatus = validationResult.Status;

					if (resultStatus == PurchaseValidationStatus.Ok)
					{
						// The purchase validation succeeded.
						ConfirmPendingPurchase(product);
						SetPurchaseCompleted(product, transactionInfo, validationResult);
					}
					else if (resultStatus == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed: confirm to avoid processing it again.
						ConfirmPendingPurchase(product);
						SetPurchaseFailed(product, StorePurchaseError.ReceiptValidationFailed, storeId);
					}
					else
					{
						// Need to re-validate the purchase: do not confirm.
						SetPurchaseFailed(product, StorePurchaseError.ReceiptValidationNotAvailable, storeId);
					}
				}
			}
			catch (Exception e)
			{
				// NOTE: Should not really get here (do we need to confirm it in this case?).
				ConfirmPendingPurchase(product);
				SetPurchaseFailed(product, StorePurchaseError.ReceiptValidationFailed, storeId, e);
			}
		}

		private void SetPurchaseCompleted(Product product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(product, transactionInfo, validationResult);

			if (_purchaseOpCs != null)
			{
				_purchaseOpCs.SetResult(result);
			}
			else
			{
				InvokePurchaseCompleted(result);
			}
		}

		private void SetPurchaseFailed(Product product, StorePurchaseError failReason, string storeId, Exception innerException = null)
		{
			if (_purchaseOpCs != null)
			{
				if (failReason == StorePurchaseError.UserCanceled)
				{
					_purchaseOpCs.SetCanceled();
				}
				else if (innerException != null)
				{
					_purchaseOpCs.SetException(new StorePurchaseException(failReason, product, storeId, innerException));
				}
				else
				{
					_purchaseOpCs.SetException(new StorePurchaseException(failReason, product, storeId));
				}
			}
			else
			{
				InvokePurchaseFailed(product, failReason, storeId, innerException);
			}
		}

		#endregion
	}
}
