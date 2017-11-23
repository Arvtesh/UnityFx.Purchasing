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
			_console.TraceEvent(TraceEventType.Verbose, TraceEventInitialize, "OnInitialized");

			try
			{
				// Have to initialize the _products collection here rather than in InitializeAsync() call
				// because when restoring purchases ProcessPurchase() (needs _products initialized)
				// might be called before InitializeAsync() resumes execution.
				foreach (var product in controller.products.all)
				{
					if (_products.TryGetValue(product.definition.id, out var userProduct))
					{
						userProduct.Metadata = product.metadata;
					}
				}

				_storeController = controller;
				_initializeOpCs.SetResult(null);

				InvokeInitializeCompleted(TraceEventInitialize);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventInitialize, e);
				_initializeOpCs.SetException(e);
			}
		}

		private void OnInitializeFailed(InitializationFailureReason error)
		{
			_console.TraceEvent(TraceEventType.Verbose, TraceEventInitialize, "OnInitializeFailed: " + error);

			try
			{
				_initializeOpCs.SetException(new StoreInitializeException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventInitialize, e);
			}
		}

		private PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			var transaction = _purchaseOperation;

			if (transaction == null)
			{
				var productId = args.purchasedProduct.definition.id;
				transaction = new PurchaseOperation(this, _console, productId, true);
				transaction.Initialize();
			}

			return transaction.ProcessPurchase(args);
		}

		private void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var transaction = _purchaseOperation;

			if (transaction == null)
			{
				var productId = product?.definition.id ?? "null";
				transaction = new PurchaseOperation(this, _console, productId, true);
				transaction.Initialize();
			}

			transaction.PurchaseFailed(product, failReason);
		}

		private void OnFetch()
		{
			// Quick return if the store has been disposed.
			if (IsDisposed)
			{
				return;
			}

			_console.TraceEvent(TraceEventType.Verbose, TraceEventFetch, "OnFetch");

			try
			{
				foreach (var product in _storeController.products.all)
				{
					if (_products.TryGetValue(product.definition.id, out var userProduct))
					{
						userProduct.Metadata = product.metadata;
					}
				}

				_fetchOpCs.SetResult(null);

				InvokeInitializeCompleted(TraceEventFetch);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventFetch, e);
				_fetchOpCs.SetException(e);
			}
		}

		private void OnFetchFailed(InitializationFailureReason error)
		{
			// Quick return if the store has been disposed.
			if (IsDisposed)
			{
				return;
			}

			_console.TraceEvent(TraceEventType.Verbose, TraceEventFetch, "OnFetchFailed: " + error);

			try
			{
				_fetchOpCs.SetException(new StoreInitializeException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventFetch, e);
			}
		}

		#endregion
	}
}
