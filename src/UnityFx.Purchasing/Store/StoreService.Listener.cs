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

		#region IStoreListener

		/// <summary>
		/// Implementation of <see cref="IStoreListener"/>.
		/// </summary>
		/// <remarks>
		/// Just forwards calls to private methods of the parent class.
		/// </remarks>
		private sealed class StoreListener : IStoreListener
		{
			private StoreService _parentStore;

			public StoreListener(StoreService service)
			{
				_parentStore = service;
			}

			public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
			{
				Debug.Assert(controller != null);
				Debug.Assert(extensions != null);

				if (!_parentStore.IsDisposed)
				{
					_parentStore.OnInitialized(controller, extensions);
				}
			}

			public void OnInitializeFailed(InitializationFailureReason error)
			{
				if (!_parentStore.IsDisposed)
				{
					_parentStore.OnInitializeFailed(error);
				}
			}

			public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
			{
				if (!_parentStore.IsDisposed)
				{
					_parentStore.OnPurchaseFailed(product, reason);
				}
			}

			public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
			{
				Debug.Assert(args != null);
				Debug.Assert(args.purchasedProduct != null);

				if (!_parentStore.IsDisposed)
				{
					return _parentStore.ProcessPurchase(args);
				}

				return PurchaseProcessingResult.Pending;
			}
		}

		private void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			_storeController = controller;
			_products.Initialize(controller);
			_initializeOp?.OnInitialized(controller, extensions);
		}

		private void OnInitializeFailed(InitializationFailureReason error)
		{
			_initializeOp?.OnInitializeFailed(error);
		}

		private PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			var transaction = _purchaseOp;

			if (transaction == null)
			{
				var productId = args.purchasedProduct.definition.id;
				transaction = new PurchaseOperation(this, _console, productId, true);
			}

			return transaction.ProcessPurchase(args);
		}

		private void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var transaction = _purchaseOp;

			if (transaction == null)
			{
				var productId = product?.definition.id ?? "null";
				transaction = new PurchaseOperation(this, _console, productId, true);
			}

			transaction.PurchaseFailed(product, failReason);
		}

		#endregion
	}
}
