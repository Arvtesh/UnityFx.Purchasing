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

		public PurchaseOperation(StoreService storeService, TraceSource console, string productId, bool restored)
			: base(console, TraceEventId.Purchase, restored ? "auto-restored" : string.Empty, productId)
		{
			Debug.Assert(storeService != null);
			Debug.Assert(productId != null);

			_storeService = storeService;
			_console = console;
			_productId = productId;
			_restored = restored;
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

		public FailedPurchaseResult GetFailedResult(Product product, StorePurchaseError reason, Exception e)
		{
			return new FailedPurchaseResult(_productId, product, reason, e);
		}

		public FailedPurchaseResult GetFailedResult(StorePurchaseError reason, Exception e)
		{
			return new FailedPurchaseResult(_productId, _transaction, null, reason, e);
		}

		public void SetPurchaseCompleted(PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(_transaction, validationResult);
			_storeService.InvokePurchaseCompleted(_productId, result);
			SetResult(result);
		}

		public void SetPurchaseFailed(StorePurchaseError failReason)
		{
			SetPurchaseFailed(failReason);
		}

		public void SetPurchaseFailed(Product product, StorePurchaseError failReason)
		{
			var result = new FailedPurchaseResult(_productId, product, failReason, null);
			_storeService.InvokePurchaseFailed(result);

			if (failReason == StorePurchaseError.UserCanceled)
			{
				SetCanceled();
			}
			else
			{
				SetException(new StorePurchaseException(result));
			}
		}

		public void SetPurchaseFailed(PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e)
		{
			var result = new FailedPurchaseResult(_productId, _transaction, validationResult, failReason, e);
			_storeService.InvokePurchaseFailed(result);
			SetException(new StorePurchaseException(result));
		}

		#endregion

		#region implementation
		#endregion
	}
}
