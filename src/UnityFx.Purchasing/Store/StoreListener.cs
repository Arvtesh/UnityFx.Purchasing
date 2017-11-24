// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreListener"/>.
	/// </summary>
	internal sealed class StoreListener : IStoreListener
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;

		private InitializeOperation _initializeOp;
		private FetchOperation _fetchOp;
		private PurchaseOperation _purchaseOp;

		#endregion

		#region interface
		
		public StoreListener(StoreService storeService, TraceSource console)
		{
			_storeService = storeService;
			_console = console;
		}

		public void SetInitializeOp(InitializeOperation op)
		{
			_initializeOp = op;
		}

		public void SetFetchOp(FetchOperation op)
		{
			_fetchOp = op;
		}

		public void SetPurchaseOp(PurchaseOperation op)
		{
			_purchaseOp = op;
		}

		#endregion

		#region IStoreListener

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Assert(controller != null);
			Debug.Assert(extensions != null);
			Debug.Assert(_initializeOp != null);

			if (!_storeService.IsDisposed)
			{
				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Initialize, "OnInitialized");
					_storeService.SetStoreController(controller);
					_initializeOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Initialize, e);
					_initializeOp?.TrySetException(e);
				}
				finally
				{
					_initializeOp = null;
				}
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.Assert(_initializeOp != null);

			if (!_storeService.IsDisposed)
			{
				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Initialize, "OnInitializeFailed: " + error);
					_initializeOp.SetException(new StoreFetchException(error));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Initialize, e);
					_initializeOp?.TrySetException(e);
				}
				finally
				{
					_initializeOp = null;
				}
			}
		}

		public void OnFetch()
		{
			if (!_storeService.IsDisposed)
			{
				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Fetch, "OnFetch");
					_fetchOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Fetch, e);
					_fetchOp?.TrySetException(e);
				}
				finally
				{
					_fetchOp = null;
				}
			}
		}

		public void OnFetchFailed(InitializationFailureReason error)
		{
			if (!_storeService.IsDisposed)
			{
				_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Fetch, "OnFetchFailed: " + error);

				try
				{
					_fetchOp.SetException(new StoreFetchException(error));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Fetch, e);
					_fetchOp?.TrySetException(e);
				}
				finally
				{
					_fetchOp = null;
				}
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			if (!_storeService.IsDisposed)
			{
				try
				{
					var product = args.purchasedProduct;
					var productId = product.definition.id;

					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Purchase, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Purchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = new PurchaseOperation(_storeService, _console, productId, true);
					}

					return _purchaseOp.ProcessPurchase(args);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Purchase, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_purchaseOp = null;
				}
			}

			return PurchaseProcessingResult.Pending;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			if (!_storeService.IsDisposed)
			{
				try
				{
					var productId = product?.definition.id ?? "null";
					_console.TraceEvent(TraceEventType.Verbose, (int)StoreService.TraceEventId.Purchase, $"OnPurchaseFailed: {productId}, reason={reason}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = new PurchaseOperation(_storeService, _console, productId, true);
					}

					_purchaseOp.SetPurchaseFailed(product, StoreService.GetPurchaseError(reason), null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)StoreService.TraceEventId.Purchase, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_purchaseOp = null;
				}
			}
		}

		#endregion
	}
}
