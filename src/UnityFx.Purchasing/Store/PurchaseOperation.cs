// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents a purchase operation.
	/// </summary>
	/// <remarks>
	/// The class stored all transaction-related data. The transaction begins when the class instance is created
	/// and ends on <see cref="Dispose"/> call.
	/// </remarks>
	internal class PurchaseOperation : IDisposable
	{
		#region data

		private const int _traceEventId = (int)StoreService.TraceEventId.Purchase;

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly string _productId;
		private readonly bool _restored;

		private IStoreProduct _product;
		private TaskCompletionSource<PurchaseResult> _purchaseOpCs;
		private bool _disposed;
		private bool _success;

		#endregion

		#region interface

		public string ProductId => _productId;

		public IStoreProduct Product => _product;

		public PurchaseOperation(StoreService storeService, TraceSource console, string productId, bool restored)
		{
			Debug.Assert(storeService != null);
			Debug.Assert(productId != null);

			_storeService = storeService;
			_console = console;
			_productId = productId;
			_restored = restored;

			if (restored)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventId, StoreService.GetEventName(_traceEventId) + " (auto-restored): " + productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventId, StoreService.GetEventName(_traceEventId) + ": " + productId);
			}

			storeService.InvokePurchaseInitiated(productId, restored);
		}

		public Product Initialize()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_storeService.Controller != null);
			Debug.Assert(_storeService.Products != null);

			if (_storeService.Products.TryGetValue(_productId, out _product))
			{
				return _storeService.Controller.products.WithID(_productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Warning, _traceEventId, "No product found for id: " + _productId);
			}

			return null;
		}

		public Task<PurchaseResult> Purchase(Product product)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(product != null);
			Debug.Assert(product.definition.id == _productId);
			Debug.Assert(_purchaseOpCs == null);
			Debug.Assert(_storeService.Controller != null);

			_console.TraceEvent(TraceEventType.Verbose, _traceEventId, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
			_purchaseOpCs = new TaskCompletionSource<PurchaseResult>(product);
			_storeService.Controller.InitiatePurchase(product);

			return _purchaseOpCs.Task;
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			var product = args.purchasedProduct;
			var productId = product.definition.id;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "ProcessPurchase: " + productId);
				_console.TraceEvent(TraceEventType.Verbose, _traceEventId, $"Receipt ({productId}): {product.receipt ?? "null"}");

				// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
				// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
				if (_restored || _productId == productId)
				{
					var transactionInfo = new StoreTransaction(product, _restored);

					if (string.IsNullOrEmpty(transactionInfo.Receipt))
					{
						SetPurchaseFailed(transactionInfo, null, StorePurchaseError.ReceiptNullOrEmpty);
					}
					else
					{
						ValidatePurchase(transactionInfo);
						return PurchaseProcessingResult.Pending;
					}
				}
			}
			catch (Exception e)
			{
				SetPurchaseFailed(new StoreTransaction(product, _restored), null, StorePurchaseError.Unknown, e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public void PurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			Debug.Assert(!_disposed);

			_console.TraceEvent(TraceEventType.Verbose, _traceEventId, $"OnPurchaseFailed: {_productId}, reason={failReason}");
			SetPurchaseFailed(new StoreTransaction(product, _restored), null, StoreService.GetPurchaseError(failReason), null);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_purchaseOpCs = null;
				_product = null;

				if (_success)
				{
					_console.TraceEvent(TraceEventType.Stop, _traceEventId, StoreService.GetEventName(_traceEventId) + " completed: " + _productId);
				}
				else
				{
					_console.TraceEvent(TraceEventType.Stop, _traceEventId, StoreService.GetEventName(_traceEventId) + " failed: " + _productId);
				}
			}
		}

		#endregion

		#region implementation

		private async void ValidatePurchase(StoreTransaction transactionInfo)
		{
			var product = transactionInfo.Product;
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventId, $"ValidatePurchase: {product.definition.id}, transactionId = {product.transactionID}");

				var validationResult = await _storeService.ValidatePurchase(_product, transactionInfo);

				// Do nothing if the store has been disposed while we were waiting for validation.
				if (!_disposed)
				{
					if (validationResult == null)
					{
						// No result returned from the validator means validation succeeded.
						ConfirmPendingPurchase(product);
						SetPurchaseCompleted(transactionInfo, validationResult);
					}
					else
					{
						resultStatus = validationResult.Status;

						if (resultStatus == PurchaseValidationStatus.Ok)
						{
							// The purchase validation succeeded.
							ConfirmPendingPurchase(product);
							SetPurchaseCompleted(transactionInfo, validationResult);
						}
						else if (resultStatus == PurchaseValidationStatus.Failure)
						{
							// The purchase validation failed: confirm to avoid processing it again.
							ConfirmPendingPurchase(product);
							SetPurchaseFailed(transactionInfo, validationResult, StorePurchaseError.ReceiptValidationFailed);
						}
						else
						{
							// Need to re-validate the purchase: do not confirm.
							SetPurchaseFailed(transactionInfo, validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
						}
					}
				}
			}
			catch (Exception e)
			{
				// NOTE: Should not really get here (do we need to confirm it in this case?).
				if (!_disposed)
				{
					ConfirmPendingPurchase(product);
					SetPurchaseFailed(transactionInfo, null, StorePurchaseError.ReceiptValidationFailed, e);
				}
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventId, "ConfirmPendingPurchase: " + product.definition.id);
			_storeService.Controller.ConfirmPendingPurchase(product);
		}

		private void SetPurchaseCompleted(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(_product, transactionInfo, validationResult);

			_success = true;

			if (_purchaseOpCs != null)
			{
				_purchaseOpCs.SetResult(result);
				_purchaseOpCs = null;
			}
			else
			{
				try
				{
					_storeService.InvokePurchaseCompleted(_productId, result);
				}
				finally
				{
					Dispose();
				}
			}
		}

		private void SetPurchaseFailed(StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e = null)
		{
			var result = new PurchaseResult(_product, transactionInfo, validationResult);

			if (_purchaseOpCs != null)
			{
				if (failReason == StorePurchaseError.UserCanceled)
				{
					_purchaseOpCs.SetCanceled();
				}
				else if (e != null)
				{
					_purchaseOpCs.SetException(new StorePurchaseException(result, failReason, e));
				}
				else
				{
					_purchaseOpCs.SetException(new StorePurchaseException(result, failReason));
				}

				_purchaseOpCs = null;
			}
			else
			{
				try
				{
					_storeService.InvokePurchaseFailed(_productId, result, failReason, e);
				}
				finally
				{
					Dispose();
				}
			}
		}

		#endregion
	}
}
