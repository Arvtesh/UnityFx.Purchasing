// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class StoreService
	{
		#region data
		#endregion

		#region interface

		internal enum TraceEventId
		{
			Default,
			Initialize,
			Fetch,
			Purchase
		}

		internal Task<PurchaseValidationResult> ValidatePurchase(IStoreProduct product, StoreTransaction transactionInfo)
		{
			return ValidatePurchaseAsync(product, transactionInfo);
		}

		internal void InvokeInitializeCompleted(int opId)
		{
			try
			{
				OnInitializeCompleted();
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

		internal void InvokeInitializeFailed(int opId, StoreInitializeError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, opId, GetEventName(opId) + " error: " + reason);

			try
			{
				OnInitializeFailed(reason, ex);
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

		internal void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			try
			{
				OnPurchaseInitiated(productId, restored);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		internal void InvokePurchaseCompleted(string productId, PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			try
			{
				_observer.OnNext(new PurchaseInfo(productId, purchaseResult, null, null));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}

			try
			{
				OnPurchaseCompleted(productId, purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		internal void InvokePurchaseFailed(string productId, PurchaseResult purchaseResult, StorePurchaseError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, TraceEventPurchase, $"{GetEventName(TraceEventPurchase)} error: {productId}, reason = {reason}");

			try
			{
				_observer.OnNext(new PurchaseInfo(productId, purchaseResult, reason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}

			try
			{
				OnPurchaseFailed(productId, purchaseResult, reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

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

		internal static string GetEventName(int eventId)
		{
			switch (eventId)
			{
				case TraceEventInitialize:
					return "Initialize";

				case TraceEventFetch:
					return "Fetch";

				case TraceEventPurchase:
					return "Purchase";
			}

			return "<Unknown>";
		}

		#endregion
	}
}
