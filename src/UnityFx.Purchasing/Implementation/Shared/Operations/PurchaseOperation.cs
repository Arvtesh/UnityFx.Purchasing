// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Purchasing;
////using UnityEngine.Purchasing.Security;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	/// <summary>
	/// A purchase operation.
	/// </summary>
	internal class PurchaseOperation : StoreOperation<PurchaseResult>
	{
		#region data

		private readonly string _productId;
		private readonly bool _restored;

		private StoreTransaction _transaction;

		#endregion

		#region interface

		public PurchaseOperation(StoreOperationContainer parent, string productId, bool restored)
			: base(parent, TraceEventId.Purchase, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert(parent != null);
			Debug.Assert(productId != null);

			_productId = productId;
			_restored = restored;

			Store.InvokePurchaseInitiated(productId, restored);
		}

		public PurchaseOperation(StoreOperationContainer parent, Product product, bool restored)
			: this(parent, product.definition.id, restored)
		{
			_transaction = new StoreTransaction(product, restored);
		}

		public void Initiate()
		{
			var product = Store.Controller.products.WithID(_productId);

			if (product != null && product.availableToPurchase)
			{
				Console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"InitiatePurchase: {_productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
				Store.Controller.InitiatePurchase(product);
			}
			else
			{
				SetFailed(product, StorePurchaseError.ProductUnavailable);
			}
		}

		public bool ProcessPurchase(Product product)
		{
			// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
			// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
			if (_restored || IsSame(product))
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
				Console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"ValidatePurchase: {_productId}, transactionId = {product.transactionID}");

				if (string.IsNullOrEmpty(_transaction.Receipt))
				{
					SetFailed(StorePurchaseError.ReceiptNullOrEmpty);
				}
				else
				{
					var validationImplemented = true;

					try
					{
						validationImplemented = Store.ValidatePurchase(_transaction, ValidateCallback);
					}
					catch (Exception e)
					{
						Console.TraceException(TraceEventId.Purchase, e);
						SetFailed(StorePurchaseError.ReceiptValidationFailed);
						return PurchaseProcessingResult.Complete;
					}

					if (validationImplemented)
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
			}
			catch (Exception e)
			{
				SetFailed(e);
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

			if (TrySetResult(result))
			{
				Store.InvokePurchaseCompleted(_productId, result);
			}
		}

		public void SetFailed(StorePurchaseError reason)
		{
			SetFailed(default(PurchaseValidationResult), reason);
		}

		public void SetFailed(Exception e)
		{
			Console.TraceException(TraceEventId.Purchase, e);

			if (TrySetException(e))
			{
				if (e is StorePurchaseException spe)
				{
					Store.InvokePurchaseFailed(new FailedPurchaseResult(spe));
				}
				else if (e is StoreFetchException sfe)
				{
					Store.InvokePurchaseFailed(GetFailedResult(StorePurchaseError.StoreNotInitialized, e));
				}
				else
				{
					Store.InvokePurchaseFailed(GetFailedResult(StorePurchaseError.Unknown, e));
				}
			}
		}

		public void SetFailed(Product product, StorePurchaseError reason, Exception e = null)
		{
			var result = new FailedPurchaseResult(_productId, product, reason, e);

			Console.TraceError(TraceEventId.Purchase, reason.ToString());

			if (reason == StorePurchaseError.UserCanceled)
			{
				if (TrySetCanceled())
				{
					Store.InvokePurchaseFailed(result);
				}
			}
			else
			{
				if (TrySetException(new StorePurchaseException(result)))
				{
					Store.InvokePurchaseFailed(result);
				}
			}
		}

		public void SetFailed(PurchaseValidationResult validationResult, StorePurchaseError reason)
		{
			var result = new FailedPurchaseResult(_productId, _transaction, validationResult, reason, null);

			Console.TraceError(TraceEventId.Purchase, reason.ToString());

			if (TrySetException(new StorePurchaseException(result)))
			{
				Store.InvokePurchaseFailed(result);
			}
		}

		public bool IsSame(Product product)
		{
			return product != null && product.definition.id == _productId;
		}

		#endregion

		#region implementation

		private void ValidateCallback(PurchaseValidationResult validationResult)
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
						SetFailed(validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
					}
					else
					{
						Console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ConfirmPendingPurchase: " + product.definition.id);
						Store.Controller.ConfirmPendingPurchase(product);

						if (status == PurchaseValidationStatus.Failure)
						{
							// The purchase validation failed.
							SetFailed(validationResult, StorePurchaseError.ReceiptValidationFailed);
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
					Console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					TrySetException(e);
				}
			}
		}

		#endregion
	}
}
