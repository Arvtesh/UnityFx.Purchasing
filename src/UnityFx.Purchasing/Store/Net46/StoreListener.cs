// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreListener"/>.
	/// </summary>
	internal sealed class StoreListener : IStoreListener, IDisposable
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;

		private InitializeOperation _initializeOp;
		private FetchOperation _fetchOp;
		private PurchaseOperation _purchaseOp;
		private bool _disposed;

		#endregion

		#region interface

		public bool IsInitializePending => _initializeOp != null;

		public Task InitializeTask => _initializeOp?.Task;

		public AsyncResult InitializeAsyncResult => throw new NotImplementedException();

		public bool IsFetchPending => _fetchOp != null;

		public Task FetchTask => _fetchOp?.Task;

		public AsyncResult FetchAsyncResult => throw new NotImplementedException();

		public bool IsPurchasePending => _purchaseOp != null;

		public Task<PurchaseResult> PurchaseTask => _purchaseOp?.Task;

		public AsyncResult<PurchaseResult> PurchaseAsyncResult => throw new NotImplementedException();

		public StoreListener(StoreService storeService, TraceSource console)
		{
			_storeService = storeService;
			_console = console;
		}

		public InitializeOperation BeginInitialize()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			_initializeOp = new InitializeOperation(_console);
			_storeService.InvokeInitializeInitiated();

			return _initializeOp;
		}

		public void EndInitialize(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);

			if (e is StoreFetchException sfe)
			{
				_storeService.InvokeInitializeFailed(StoreService.GetInitializeError(sfe.Reason), e);
			}
			else
			{
				_storeService.InvokeInitializeFailed(StoreFetchError.Unknown, e);
			}

			_initializeOp.TrySetException(e);
			_initializeOp.Dispose();
			_initializeOp = null;
		}

		public void EndInitialize()
		{
			_initializeOp.Dispose();
			_initializeOp = null;
		}

		public FetchOperation BeginFetch()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_purchaseOp == null);

			_fetchOp = new FetchOperation(_console);
			_storeService.InvokeFetchInitiated();

			return _fetchOp;
		}

		public void EndFetch(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);

			if (e is StoreFetchException sfe)
			{
				_storeService.InvokeFetchFailed(StoreService.GetInitializeError(sfe.Reason), e);
			}
			else
			{
				_storeService.InvokeFetchFailed(StoreFetchError.Unknown, e);
			}

			_fetchOp.TrySetException(e);
			_fetchOp.Dispose();
			_fetchOp = null;
		}

		public void EndFetch()
		{
			_fetchOp.Dispose();
			_fetchOp = null;
		}

		public PurchaseOperation BeginPurchase(string productId, bool isRestored)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_purchaseOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			_purchaseOp = new PurchaseOperation(_storeService, _console, productId, isRestored);
			_storeService.InvokePurchaseInitiated(productId, isRestored);

			return _purchaseOp;
		}

		public void EndPurchase(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			_storeService.InvokePurchaseFailed(new FailedPurchaseResult(_purchaseOp.ProductId, null, null, StorePurchaseError.Unknown, e));
			_purchaseOp.TrySetException(e);
			_purchaseOp.Dispose();
			_purchaseOp = null;
		}

		public void EndPurchase()
		{
			_purchaseOp.Dispose();
			_purchaseOp = null;
		}

		#endregion

		#region IStoreListener

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Assert(controller != null);
			Debug.Assert(extensions != null);

			if (!_disposed)
			{
				Debug.Assert(_initializeOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Initialize, "OnInitialized");
					_storeService.SetStoreController(controller);
					_storeService.InvokeInitializeCompleted();
					_initializeOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
					_initializeOp.TrySetException(e);
				}
				finally
				{
					_initializeOp.Dispose();
					_initializeOp = null;
				}
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_initializeOp != null);

				try
				{
					var e = new StoreFetchException(error);

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Initialize, "OnInitializeFailed: " + error);
					_storeService.InvokeInitializeFailed(StoreService.GetInitializeError(error), e);
					_initializeOp.SetException(e);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
					_initializeOp.TrySetException(e);
				}
				finally
				{
					_initializeOp.Dispose();
					_initializeOp = null;
				}
			}
		}

		public void OnFetch()
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Fetch, "OnFetch");
					_storeService.InvokeFetchCompleted();
					_fetchOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
					_fetchOp.TrySetException(e);
				}
				finally
				{
					_fetchOp.Dispose();
					_fetchOp = null;
				}
			}
		}

		public void OnFetchFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					var e = new StoreFetchException(error);

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Fetch, "OnFetchFailed: " + error);
					_storeService.InvokeFetchFailed(StoreService.GetInitializeError(error), e);
					_fetchOp.SetException(e);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
					_fetchOp.TrySetException(e);
				}
				finally
				{
					_fetchOp.Dispose();
					_fetchOp = null;
				}
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			if (!_disposed)
			{
				try
				{
					var product = args.purchasedProduct;
					var productId = product.definition.id;

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = BeginPurchase(productId, true);
					}

					return _purchaseOp.ProcessPurchase(args);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
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
			if (!_disposed)
			{
				try
				{
					var productId = product?.definition.id ?? "null";
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"OnPurchaseFailed: {productId}, reason={reason}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = BeginPurchase(productId, true);
					}

					_purchaseOp.SetPurchaseFailed(product, StoreService.GetPurchaseError(reason), null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_purchaseOp = null;
				}
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				if (_purchaseOp != null)
				{
					_storeService.InvokePurchaseFailed(new FailedPurchaseResult(_purchaseOp.ProductId, null, null, StorePurchaseError.StoreDisposed, null));
					_purchaseOp.Dispose();
					_purchaseOp = null;
				}

				if (_fetchOp != null)
				{
					_storeService.InvokeFetchFailed(StoreFetchError.StoreDisposed, null);
					_fetchOp.Dispose();
					_fetchOp = null;
				}

				if (_initializeOp != null)
				{
					_storeService.InvokeInitializeFailed(StoreFetchError.StoreDisposed, null);
					_initializeOp.Dispose();
					_initializeOp = null;
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
