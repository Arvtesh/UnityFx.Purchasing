// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents a purchase operation.
	/// </summary>
	internal class PurchaseOperation : StoreOperation<PurchaseResult>
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly string _productId;
		private readonly bool _restored;

		#endregion

		#region interface

		public string ProductId => _productId;

		public PurchaseOperation(StoreService storeService, TraceSource console, string productId, bool restored)
			: base(console, StoreService.TraceEventId.Purchase, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert(storeService != null);
			Debug.Assert(productId != null);

			_storeService = storeService;
			_console = console;
			_productId = productId;
			_restored = restored;

			_storeService.InvokePurchaseInitiated(productId, restored);
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			var product = args.purchasedProduct;
			var productId = product.definition.id;

			try
			{
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

		public void SetPurchaseCompleted(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(transactionInfo, validationResult);

			if (_restored)
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
			else
			{
				SetResult(result);
			}
		}

		public void SetPurchaseFailed(Product product, StorePurchaseError failReason, Exception e = null)
		{
			SetPurchaseFailed(new StoreTransaction(product, _restored), null, failReason, e);
		}

		public void SetPurchaseFailed(StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e = null)
		{
			var result = new PurchaseResult(transactionInfo, validationResult);

			if (_restored)
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
			else
			{
				if (failReason == StorePurchaseError.UserCanceled)
				{
					SetCanceled();
				}
				else if (e != null)
				{
					SetException(new StorePurchaseException(result, failReason, e));
				}
				else
				{
					SetException(new StorePurchaseException(result, failReason));
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
				_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Purchase, $"ValidatePurchase: {product.definition.id}, transactionId = {product.transactionID}");

				var validationResult = await _storeService.ValidatePurchase(transactionInfo);

				// Do nothing if the store has been disposed while we were waiting for validation.
				if (!IsDisposed)
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
				if (!IsDisposed)
				{
					ConfirmPendingPurchase(product);
					SetPurchaseFailed(transactionInfo, null, StorePurchaseError.ReceiptValidationFailed, e);
				}
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Purchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeService.Controller.ConfirmPendingPurchase(product);
		}

		#endregion
	}
}
