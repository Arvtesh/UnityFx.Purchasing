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

		private void InvokePurchaseCompleted(PurchaseResult purchaseResult)
		{
			try
			{
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult.TransactionInfo, purchaseResult.ValidationResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "Purchase completed: " + purchaseResult.Product.definition.id);
			}
		}

		private void InvokePurchaseFailed(Product product, StorePurchaseError failReason, string storeId, Exception innerException = null)
		{
			var productId = product != null ? product.definition.id : "<null>";

			if (innerException != null)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, innerException);
			}

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"Purchase error: {productId}, reason={failReason}");

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(product, failReason, storeId, _purchaseOpCs == null));
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
			_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeController.ConfirmPendingPurchase(product);
		}

		#endregion
	}
}
