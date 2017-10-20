// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class StoreService
	{
		#region data
		#endregion

		#region interface

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

		private void InvokePurchaseInitiated(string productId)
		{
			_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "Purchase: " + productId);

			try
			{
				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		private void InvokePurchaseFailed(Product product, StorePurchaseError failReason, string storeId, Exception innerException = null)
		{
			var productId = product != null ? product.definition.id : "<null>";

			try
			{
				_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"PurchaseFailed: {productId}, reason={failReason}");

				// Finish the corresponding Task.
				if (failReason == StorePurchaseError.UserCanceled)
				{
					_purchaseOpCs?.SetCanceled();
				}
				else if (innerException != null)
				{
					_purchaseOpCs?.SetException(new StorePurchaseException(failReason, product, storeId, innerException));
				}
				else
				{
					_purchaseOpCs?.SetException(new StorePurchaseException(failReason, product, storeId));
				}

				// Trigger completion event.
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(product, failReason, storeId, _purchaseOpCs == null));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "PurchaseFailed: " + productId);
			}
		}

		private void InvokePurchaseCompleted(Product product, StoreTransaction transaction, PurchaseValidationResult validationResult)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);

				// Confirm the purchase on the store.
				_storeController.ConfirmPendingPurchase(product);

				// Finish the corresponding Task.
				_purchaseOpCs?.SetResult(transaction);

				// Trigger completion event.
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(transaction, validationResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "PurchaseCompleted: " + product.definition.id);
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeController.ConfirmPendingPurchase(product);
		}

		#endregion
	}
}
