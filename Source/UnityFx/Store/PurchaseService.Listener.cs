// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class PurchaseService : IStoreListener
	{
		#region data
		#endregion

		#region IStoreListener

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventInitialize, "OnInitialized");
			_storeController = controller;
			_initializeOpCs?.SetResult(null);
			_initializeOpCs = null;
			_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialized");
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventInitialize, "OnInitializeFailed: " + error);
			_initializeOpCs?.SetException(new StoreInitializeException(error));
			_initializeOpCs = null;
			_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize failed");
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
						_purchaseOpCs?.SetException(new StorePurchaseException(StorePurchaseError.ReceiptNullOrEmpty, product, storeId));
					}
					else if (_delegate != null)
					{
						ValidatePurchase(product, storeId, nativeReceipt);
						return PurchaseProcessingResult.Pending;
					}
					else
					{
						InvokePurchaseCompleted(product, storeId, null);
					}
				}
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
				_purchaseOpCs?.SetException(new StorePurchaseException(StorePurchaseError.Unknown, null, null, e));
			}

			ConfirmPendingPurchase(args.purchasedProduct, true);
			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"OnPurchaseFailed: {product.definition.id}, reason={failReason}");

			if (failReason == PurchaseFailureReason.UserCancelled)
			{
				_purchaseOpCs?.SetCanceled();
			}
			else
			{
				_purchaseOpCs?.SetException(new StorePurchaseException(failReason.ToString()));
			}
		}

		#endregion

		#region implementation

		private async void ValidatePurchase(Product product, string storeId, string nativeReceipt)
		{
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId={product.transactionID}");

				var validationResult = await _delegate.ValidatePurchase(product, storeId, nativeReceipt);

				if (validationResult == null)
				{
					ConfirmPendingPurchase(product);
					InvokePurchaseCompleted(product, storeId, null);
				}
				else
				{
					resultStatus = validationResult.Status;

					if (resultStatus == PurchaseValidationStatus.Ok)
					{
						ConfirmPendingPurchase(product);
						InvokePurchaseCompleted(product, storeId, validationResult);
					}
					else if (resultStatus == PurchaseValidationStatus.Failure)
					{
						ConfirmPendingPurchase(product);
						_purchaseOpCs?.SetException(new StorePurchaseException(StorePurchaseError.ReceiptValidationFailed, product, storeId));
					}
					else
					{
						_purchaseOpCs?.SetException(new StorePurchaseException(StorePurchaseError.ReceiptValidationNotAvailable, product, storeId));
					}
				}
			}
			catch (Exception e)
			{
				ConfirmPendingPurchase(product);
				_purchaseOpCs?.SetException(new StorePurchaseException(StorePurchaseError.ReceiptValidationFailed, product, storeId, e));
			}
		}

		#endregion
	}
}
