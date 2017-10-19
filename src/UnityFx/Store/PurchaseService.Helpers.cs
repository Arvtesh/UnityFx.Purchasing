// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class PurchaseService
	{
		#region data
		#endregion

		#region implementation

		private void InvokePurchaseInitiated(string productId)
		{
			_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, "PurchaseInitiated: " + productId);

			try
			{
				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		private void InvokePurchaseFailed(Product product, StorePurchaseError failReason, string storeId)
		{
			var productId = product != null ? product.definition.id : "<null>";
			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"PurchaseFailed: {productId}, reason={failReason}");

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(storeId, product, failReason, _purchaseOpCs == null));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}

			_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "PurchaseFailed: " + productId);
		}

		private void InvokePurchaseCompleted(Product product, string storeId, PurchaseValidationResult validationResult)
		{
			_purchaseOpCs?.SetResult(product);

			try
			{
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(product, storeId, _purchaseOpCs == null, validationResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}

			_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, "PurchaseCompleted: " + product.definition.id);
		}

		private Task<Product> InitiatePurchase(Product product)
		{
			_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
			_purchaseOpCs = new TaskCompletionSource<Product>(product);
			_storeController.InitiatePurchase(product);
			return _purchaseOpCs.Task;
		}

		private void ConfirmPendingPurchase(Product product, bool justLog = false)
		{
			_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);

			if (!justLog)
			{
				_storeController.ConfirmPendingPurchase(product);
			}
		}

		#endregion
	}
}
