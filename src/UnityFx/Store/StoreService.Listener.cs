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
			Debug.Assert(controller != null);
			Debug.Assert(extensions != null);

			try
			{
				_console.TraceEvent(TraceEventType.Information, _traceEventInitialize, "OnInitialized");

				foreach (var product in controller.products.all)
				{
					_products[product.definition.id].Metadata = product.metadata;
				}

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
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			var product = args.purchasedProduct;
			var productId = product.definition.id;
			var isRestored = _purchaseOpCs == null;

			try
			{
				// If the purchase operation has been auto-restored _purchaseOpCs would be null.
				if (isRestored)
				{
					InvokePurchaseInitiated(productId, true);
					InitializeTransaction(productId);
				}

				_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ProcessPurchase: " + productId);
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

				// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
				// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
				if (isRestored || _purchaseOpCs.Task.AsyncState.Equals(product))
				{
					var transactionInfo = new StoreTransaction(product, isRestored);

					if (string.IsNullOrEmpty(transactionInfo.Receipt))
					{
						SetPurchaseFailed(_purchaseProduct, transactionInfo, null, StorePurchaseError.ReceiptNullOrEmpty);
					}
					else
					{
						ValidatePurchase(product, transactionInfo);
						return PurchaseProcessingResult.Pending;
					}
				}
			}
			catch (Exception e)
			{
				SetPurchaseFailed(_purchaseProduct, new StoreTransaction(product, isRestored), null, StorePurchaseError.Unknown, e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var productId = product != null ? product.definition.id : "null";
			var isRestored = _purchaseOpCs == null;

			// If the purchase operation has been auto-restored, _purchaseOpCs would be null.
			if (isRestored)
			{
				InvokePurchaseInitiated(productId, true);
				InitializeTransaction(productId);
			}

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"OnPurchaseFailed: {productId}, reason={failReason}");

			SetPurchaseFailed(_purchaseProduct, new StoreTransaction(product, isRestored), null, GetPurchaseError(failReason), null);
		}

		#endregion

		#region implementation

		private async void ValidatePurchase(Product product, StoreTransaction transactionInfo)
		{
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId={product.transactionID}");

				var validationResult = await _delegate.ValidatePurchaseAsync(transactionInfo);

				if (validationResult == null)
				{
					// No result returned from the validator means validation succeeded.
					ConfirmPendingPurchase(product);
					SetPurchaseCompleted(_purchaseProduct, transactionInfo, validationResult);
				}
				else
				{
					resultStatus = validationResult.Status;

					if (resultStatus == PurchaseValidationStatus.Ok)
					{
						// The purchase validation succeeded.
						ConfirmPendingPurchase(product);
						SetPurchaseCompleted(_purchaseProduct, transactionInfo, validationResult);
					}
					else if (resultStatus == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed: confirm to avoid processing it again.
						ConfirmPendingPurchase(product);
						SetPurchaseFailed(_purchaseProduct, transactionInfo, validationResult, StorePurchaseError.ReceiptValidationFailed);
					}
					else
					{
						// Need to re-validate the purchase: do not confirm.
						SetPurchaseFailed(_purchaseProduct, transactionInfo, validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
					}
				}
			}
			catch (Exception e)
			{
				// NOTE: Should not really get here (do we need to confirm it in this case?).
				ConfirmPendingPurchase(product);
				SetPurchaseFailed(_purchaseProduct, transactionInfo, null, StorePurchaseError.ReceiptValidationFailed, e);
			}
		}

		private void SetPurchaseCompleted(IStoreProduct product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(product, transactionInfo, validationResult);

			if (_purchaseOpCs != null)
			{
				_purchaseOpCs.SetResult(result);
			}
			else
			{
				InvokePurchaseCompleted(result);
				ReleaseTransaction();
			}
		}

		private void SetPurchaseFailed(IStoreProduct product, StoreTransaction transactioInfo, PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception innerException = null)
		{
			if (_purchaseOpCs != null)
			{
				if (failReason == StorePurchaseError.UserCanceled)
				{
					_purchaseOpCs.SetCanceled();
				}
				else if (innerException != null)
				{
					_purchaseOpCs.SetException(new StorePurchaseException(product, transactioInfo, validationResult, failReason, innerException));
				}
				else
				{
					_purchaseOpCs.SetException(new StorePurchaseException(product, transactioInfo, validationResult, failReason));
				}
			}
			else
			{
				InvokePurchaseFailed(product, transactioInfo, validationResult, failReason, innerException);
				ReleaseTransaction();
			}
		}

		#endregion
	}
}
