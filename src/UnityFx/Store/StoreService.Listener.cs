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

				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, "ProcessPurchase: " + productId);
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

				// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
				// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
				if (_purchaseOpCs == null || _purchaseOpCs.Task.AsyncState.Equals(product))
				{
					var nativeReceipt = ProductExtensions.GetNativeReceipt(product, out string storeId);

					if (string.IsNullOrEmpty(nativeReceipt))
					{
						InvokePurchaseFailed(args.purchasedProduct, StorePurchaseError.ReceiptNullOrEmpty, storeId);
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
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
				InvokePurchaseFailed(args.purchasedProduct, StorePurchaseError.Unknown, null, e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"OnPurchaseFailed: {product.definition.id}, reason={failReason}");
			InvokePurchaseFailed(product, GetPurchaseError(failReason), null);
		}

		#endregion

		#region implementation

		private async void ValidatePurchase(Product product, string storeId, string nativeReceipt)
		{
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId={product.transactionID}");

				var validationResult = await _delegate.ValidatePurchaseAsync(product, storeId, nativeReceipt);

				if (validationResult == null)
				{
					// No result returned from the validator means validation succeeded.
					InvokePurchaseCompleted(product, storeId, null);
				}
				else
				{
					resultStatus = validationResult.Status;

					if (resultStatus == PurchaseValidationStatus.Ok)
					{
						// The purchase validation succeeded.
						InvokePurchaseCompleted(product, storeId, validationResult);
					}
					else if (resultStatus == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed: confirm to avoid processing it again.
						ConfirmPendingPurchase(product);
						InvokePurchaseFailed(product, StorePurchaseError.ReceiptValidationFailed, storeId);
					}
					else
					{
						// Need to re-validate the purchase: do not confirm.
						InvokePurchaseFailed(product, StorePurchaseError.ReceiptValidationNotAvailable, storeId);
					}
				}
			}
			catch (Exception e)
			{
				// NOTE: Should not really get here (do we need to confirm it in this case?).
				ConfirmPendingPurchase(product);
				InvokePurchaseFailed(product, StorePurchaseError.ReceiptValidationFailed, storeId, e);
			}
		}

		#endregion
	}
}
