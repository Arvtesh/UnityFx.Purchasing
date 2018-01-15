// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A purchase operation.
	/// </summary>
	internal class PurchaseOperation : StoreOperation<PurchaseResult>
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly string _productId;
		private readonly bool _restored;

		private StoreTransaction _transaction;

		#endregion

		#region interface

		public string ProductId => _productId;

		public StoreTransaction Transaction => _transaction;

		public PurchaseOperation(StoreOperationContainer parent, string productId, bool restored)
			: base(parent, TraceEventId.Purchase, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert(parent != null);
			Debug.Assert(productId != null);

			_storeService = parent.Store;
			_console = _storeService.TraceSource;
			_productId = productId;
			_restored = restored;

			Store.InvokePurchaseInitiated(productId, restored);
		}

		public PurchaseOperation(StoreOperationContainer parent, Product product)
			: this(parent, product.definition.id, true)
		{
			_transaction = new StoreTransaction(product, true);
		}

		public bool ProcessPurchase(Product product)
		{
			var productId = product.definition.id;

			// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
			// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
			if (_restored || _productId == productId)
			{
				_transaction = new StoreTransaction(product, _restored);
				return true;
			}

			return false;
		}

		public PurchaseProcessingResult Validate(Product product)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"ValidatePurchase: {_productId}, transactionId = {product.transactionID}");

				if (string.IsNullOrEmpty(_transaction.Receipt))
				{
					SetFailed(StorePurchaseError.ReceiptNullOrEmpty);
				}
				else if (_storeService.ValidatePurchase(_transaction, ValidatePurchaseCallback))
				{
					// Check if the validation callback was called synchronously.
					if (!IsCompleted)
					{
						return PurchaseProcessingResult.Pending;
					}
				}
				else
				{
					SetCompleted(new PurchaseValidationResult(PurchaseValidationStatus.Suppressed));
				}
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
				TrySetException(e);
			}

			return PurchaseProcessingResult.Complete;
		}

		public FailedPurchaseResult GetFailedResult(Product product, StorePurchaseError reason, Exception e)
		{
			return new FailedPurchaseResult(_productId, product, reason, e);
		}

		public FailedPurchaseResult GetFailedResult(StorePurchaseError reason, Exception e)
		{
			return new FailedPurchaseResult(_productId, _transaction, null, reason, e);
		}

		public void SetCompleted(PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(_transaction, validationResult);
			_storeService.InvokePurchaseCompleted(_productId, result);
			TrySetResult(result);
		}

		public void SetFailed(StorePurchaseError failReason)
		{
			SetFailed(null, failReason, null);
		}

		public void SetFailed(Exception e)
		{
			if (e is StorePurchaseException spe)
			{
				_storeService.InvokePurchaseFailed(new FailedPurchaseResult(spe));
			}
			else if (e is StoreFetchException sfe)
			{
				_storeService.InvokePurchaseFailed(GetFailedResult(StorePurchaseError.StoreNotInitialized, e));
			}
			else
			{
				_storeService.InvokePurchaseFailed(GetFailedResult(StorePurchaseError.Unknown, e));
			}

			TrySetException(e);
		}

		public void SetFailed(Product product, StorePurchaseError failReason)
		{
			var result = new FailedPurchaseResult(_productId, product, failReason, null);
			_storeService.InvokePurchaseFailed(result);

			if (failReason == StorePurchaseError.UserCanceled)
			{
				TrySetCanceled();
			}
			else
			{
				TrySetException(new StorePurchaseException(result));
			}
		}

		public void SetFailed(PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e)
		{
			var result = new FailedPurchaseResult(_productId, _transaction, validationResult, failReason, e);
			_storeService.InvokePurchaseFailed(result);
			TrySetException(new StorePurchaseException(result));
		}

		#endregion

		#region implementation

		private void ValidatePurchaseCallback(PurchaseValidationResult validationResult)
		{
			if (!IsCompleted)
			{
				try
				{
					if (validationResult == null)
					{
						validationResult = new PurchaseValidationResult(PurchaseValidationStatus.Suppressed);
					}

					var status = validationResult.Status;
					var product = _transaction.Product;

					if (status == PurchaseValidationStatus.NotAvailable)
					{
						// Need to re-validate the purchase: do not confirm.
						SetFailed(validationResult, StorePurchaseError.ReceiptValidationNotAvailable, null);
					}
					else
					{
						_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ConfirmPendingPurchase: " + product.definition.id);
						_storeService.Controller.ConfirmPendingPurchase(product);

						if (status == PurchaseValidationStatus.Failure)
						{
							// The purchase validation failed.
							SetFailed(validationResult, StorePurchaseError.ReceiptValidationFailed, null);
						}
						else
						{
							// The purchase validation succeeded.
							SetCompleted(validationResult);
						}
					}
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					TrySetException(e);
				}
			}
		}

		#endregion
	}
}
