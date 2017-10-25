// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class StoreService
	{
		#region data
		#endregion

		#region interface

		internal static StoreInitializeError GetInitializeError(InitializationFailureReason error)
		{
			switch (error)
			{
				case InitializationFailureReason.AppNotKnown:
					return StoreInitializeError.AppNotKnown;

				case InitializationFailureReason.NoProductsAvailable:
					return StoreInitializeError.NoProductsAvailable;

				case InitializationFailureReason.PurchasingUnavailable:
					return StoreInitializeError.PurchasingUnavailable;

				default:
					return StoreInitializeError.Unknown;
			}
		}

		internal static StorePurchaseError GetPurchaseError(PurchaseFailureReason error)
		{
			switch (error)
			{
				case PurchaseFailureReason.PurchasingUnavailable:
					return StorePurchaseError.PurchasingUnavailable;

				case PurchaseFailureReason.ExistingPurchasePending:
					return StorePurchaseError.ExistingPurchasePending;

				case PurchaseFailureReason.ProductUnavailable:
					return StorePurchaseError.ProductUnavailable;

				case PurchaseFailureReason.SignatureInvalid:
					return StorePurchaseError.SignatureInvalid;

				case PurchaseFailureReason.UserCancelled:
					return StorePurchaseError.UserCanceled;

				case PurchaseFailureReason.PaymentDeclined:
					return StorePurchaseError.PaymentDeclined;

				case PurchaseFailureReason.DuplicateTransaction:
					return StorePurchaseError.DuplicateTransaction;

				default:
					return StorePurchaseError.Unknown;
			}
		}

		#endregion

		#region implementation

		private void InvokeInitializeCompleted()
		{
			try
			{
				StoreInitialized?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize complete");
			}
		}

		private void InvokeInitializeFailed(StoreInitializeError reason, Exception ex = null)
		{
			_console.TraceData(TraceEventType.Error, _traceEventInitialize, ex);
			_console.TraceEvent(TraceEventType.Error, _traceEventInitialize, $"Initialize error: {reason}");

			try
			{
				StoreInitializationFailed?.Invoke(this, new PurchaseInitializationFailed(reason));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize failed");
			}
		}

		private void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			if (restored)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "Purchase (auto-restored): " + productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "Purchase: " + productId);
			}

			try
			{
				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, restored));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		private void InvokePurchaseCompleted(PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			try
			{
				if (_observers != null)
				{
					lock (_observers)
					{
						foreach (var item in _observers)
						{
							item.OnNext(new PurchaseInfo(purchaseResult, null, null));
						}
					}
				}
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}

			try
			{
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "Purchase completed: " + purchaseResult.Product.Definition.id);
			}
		}

		private void InvokePurchaseFailed(PurchaseResult purchaseResult, StorePurchaseError failReason, Exception ex = null)
		{
			var product = purchaseResult.TransactionInfo.Product;
			var productId = product != null ? product.definition.id : "<null>";

			if (ex != null)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, ex);
			}

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"Purchase error: {productId}, reason = {failReason}");

			try
			{
				if (_observers != null)
				{
					lock (_observers)
					{
						foreach (var item in _observers)
						{
							item.OnNext(new PurchaseInfo(purchaseResult, failReason, ex));
						}
					}
				}
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(purchaseResult, failReason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "Purchase failed: " + productId);
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			Debug.Assert(product != null);
			Debug.Assert(_storeController != null);

			_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeController.ConfirmPendingPurchase(product);
		}

		private Product InitializeTransaction(string productId)
		{
			Debug.Assert(_purchaseProduct == null);
			Debug.Assert(_storeController != null);

			if (!_products.TryGetValue(productId, out _purchaseProduct))
			{
				_console.TraceEvent(TraceEventType.Warning, _traceEventPurchase, "No product found for id: " + productId);
			}

			return _storeController.products.WithID(productId);
		}

		private void ReleaseTransaction()
		{
			_purchaseProduct = null;
			_purchaseOpCs = null;
		}

		#endregion
	}
}
