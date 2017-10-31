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

		private void InvokeInitializeCompleted(int opId)
		{
			try
			{
				StoreInitialized?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, opId, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, opId, GetEventName(opId) + " complete");
			}
		}

		private void InvokeInitializeFailed(int opId, StoreInitializeError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, opId, GetEventName(opId) + " error: " + reason);

			try
			{
				StoreInitializationFailed?.Invoke(this, new PurchaseInitializationFailed(reason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, opId, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, opId, GetEventName(opId) + " failed");
			}
		}

		private void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			if (restored)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, GetEventName(_traceEventPurchase) + " (auto-restored): " + productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, GetEventName(_traceEventPurchase) + ": " + productId);
			}

			_purchaseProductId = productId;

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

			if (_observers != null)
			{
				lock (_observers)
				{
					foreach (var item in _observers)
					{
						try
						{
							item.OnNext(new PurchaseInfo(_purchaseProductId, purchaseResult, null, null));
						}
						catch (Exception e)
						{
							_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
						}
					}
				}
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
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, GetEventName(_traceEventPurchase) + " completed: " + _purchaseProductId);
			}
		}

		private void InvokePurchaseFailed(PurchaseResult purchaseResult, StorePurchaseError failReason, Exception ex)
		{
			var product = purchaseResult.TransactionInfo?.Product;
			var productId = _purchaseProductId ?? "null";

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"{GetEventName(_traceEventPurchase)} error: {productId}, reason = {failReason}");

			if (_observers != null)
			{
				lock (_observers)
				{
					foreach (var item in _observers)
					{
						try
						{
							item.OnNext(new PurchaseInfo(productId, purchaseResult, failReason, ex));
						}
						catch (Exception e)
						{
							_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
						}
					}
				}
			}

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(productId, purchaseResult, failReason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, GetEventName(_traceEventPurchase) + " failed: " + productId);
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			Debug.Assert(product != null);
			Debug.Assert(_storeController != null);

			_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeController.ConfirmPendingPurchase(product);
		}

		private Product InitializeTransaction(string productId)
		{
			Debug.Assert(_purchaseProduct == null);
			Debug.Assert(_storeController != null);

			if (_products.TryGetValue(productId, out _purchaseProduct))
			{
				return _storeController.products.WithID(productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Warning, _traceEventPurchase, "No product found for id: " + productId);
			}

			return null;
		}

		private string GetEventName(int eventId)
		{
			switch (eventId)
			{
				case _traceEventInitialize:
					return "Initialize";

				case _traceEventFetch:
					return "Fetch";

				case _traceEventPurchase:
					return "Purchase";
			}

			return "<Unknown>";
		}

		private void ReleaseTransaction()
		{
			_purchaseProductId = null;
			_purchaseProduct = null;
			_purchaseOpCs = null;
		}

		#endregion
	}
}
