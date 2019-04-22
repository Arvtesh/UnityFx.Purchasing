// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing.Impl
{
	internal sealed class StoreListener : IStoreListener, IDisposable
	{
		#region data

		private readonly TraceSource _console;
		private readonly Action<Product> _restoredPurchaseHandler;
		private readonly Action<Product, PurchaseFailureReason> _restoredPurchaseFailedHandler;

		private IStoreController _storeController;
		private IExtensionProvider _storeExtensions;

		private TaskCompletionSource<object> _initializeOp;
		private TaskCompletionSource<object> _fetchOp;
		private TaskCompletionSource<Product> _purchaseOp;

		private int _opId;
		private string _productId;
		private bool _disposed;

		#endregion

		#region interface

		public IStoreController Controller
		{
			get
			{
				return _storeController;
			}
		}

		public IExtensionProvider Extensions
		{
			get
			{
				return _storeExtensions;
			}
		}

		public StoreListener(TraceSource traceSource, Action<Product> restoredPurchaseHandler, Action<Product, PurchaseFailureReason> restoredPurchaseFailedHandler)
		{
			_console = traceSource;
			_restoredPurchaseHandler = restoredPurchaseHandler;
			_restoredPurchaseFailedHandler = restoredPurchaseFailedHandler;
		}

		public Task InitializeAsync(ConfigurationBuilder builder, int opId, object asyncState)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_purchaseOp == null);

			var op = _initializeOp = new TaskCompletionSource<object>(asyncState);

			_opId = opId;
			UnityPurchasing.Initialize(this, builder);

			return op.Task;
		}

		public Task FetchAsync(IStoreConfig config, int opId, object asyncState)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_purchaseOp == null);

			if (config != null)
			{
				var op = _fetchOp = new TaskCompletionSource<object>(asyncState);
				var productSet = new HashSet<ProductDefinition>(config.Products);

				_opId = opId;
				_storeController.FetchAdditionalProducts(productSet, OnFetch, OnFetchFailed);

				return op.Task;
			}
			else
			{
				return Task.CompletedTask;
			}
		}

		public Task<Product> PurchaseAsync(string productId, int opId, object asyncState)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_purchaseOp == null);

			var op = _purchaseOp = new TaskCompletionSource<Product>(asyncState);

			_opId = opId;
			_productId = productId;
			_storeController.InitiatePurchase(productId);

			return op.Task;
		}

		#endregion

		#region IStoreListener

		void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Assert(controller != null);
			Debug.Assert(extensions != null);

			if (!_disposed)
			{
				Debug.Assert(_storeController == null);
				Debug.Assert(_initializeOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, _opId, nameof(IStoreListener.OnInitialized));
					_storeController = controller;
					_storeExtensions = extensions;
					_initializeOp.TrySetResult(null);
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _opId, e);
					_initializeOp.TrySetException(new StoreInitializeException(InitializationFailureReason.PurchasingUnavailable, e));
				}
				finally
				{
					_opId = 0;
					_initializeOp = null;
				}
			}
		}

		void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_initializeOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, _opId, $"{nameof(IStoreListener.OnInitializeFailed)}: {error}.");
					_initializeOp.TrySetException(new StoreInitializeException(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _opId, e);
					_initializeOp.TrySetException(new StoreInitializeException(error, e));
				}
				finally
				{
					_opId = 0;
					_initializeOp = null;
				}
			}
		}

		PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			// NOTE: The method implementation relies on non-default SynchronizationContext attached to the Unity thread. In this case
			// resolving TaskCompletionSource schedules the continuations to run on the context instead of running them inline. This behavior
			// is critical because it lets the method return before running continuations (ConfirmPendingPurchase called by the StoreService
			// might cause undefined behavior if called before ProcessPurchase returns).
			if (!_disposed)
			{
				var product = args.purchasedProduct;
				var productId = product.definition.id;

				try
				{
					var transactionId = product.transactionID ?? "null";
					var receipt = product.hasReceipt && product.receipt != null ? product.receipt : "null";

					_console.TraceEvent(TraceEventType.Verbose, _opId, $"{nameof(IStoreListener.ProcessPurchase)}: {productId}, transactionId: {transactionId}, receipt: {receipt}.");

					if (_purchaseOp == null)
					{
						// A restored transaction.
						_restoredPurchaseHandler?.Invoke(product);
					}
					else if (_productId == productId)
					{
						// Normal transaction initiated with IStoreService.PurchaseAsync() call.
						_purchaseOp.TrySetResult(product);
					}
					else
					{
						// Should not really get here. A wierd transaction initiated directly with IStoreController.InitiatePurchase() call (bypassing IStoreService API). Do not process it.
						_console.TraceEvent(TraceEventType.Warning, _opId, $"{nameof(IStoreListener.ProcessPurchase)}: unexpected product - got {productId} while {_productId} was expected.");
						_purchaseOp.TrySetException(new PurchaseException(product, PurchaseFailureReason.Unknown));
					}
				}
				catch (Exception e)
				{
					// NOTE: Should never get here. Exception means logic error in the store implementation.
					_console.TraceData(TraceEventType.Critical, _opId, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_opId = 0;
					_productId = null;
					_purchaseOp = null;
				}
			}

			return PurchaseProcessingResult.Pending;
		}

		void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			if (!_disposed)
			{
				// NOTE: In some cases product might have null value.
				if (product == null)
				{
					_console.TraceEvent(TraceEventType.Verbose, 0, $"{nameof(IStoreListener.OnPurchaseFailed)}: {reason}.");
					_console.TraceEvent(TraceEventType.Warning, 0, $"{nameof(IStoreListener.OnPurchaseFailed)} is called with null product.");
				}
				else
				{
					try
					{
						var productId = product.definition.id;

						_console.TraceEvent(TraceEventType.Verbose, _opId, $"{nameof(IStoreListener.OnPurchaseFailed)}: {reason} ({productId}).");

						if (_purchaseOp == null)
						{
							// A restored transaction.
							_restoredPurchaseFailedHandler?.Invoke(product, reason);
						}
						else if (_productId == productId)
						{
							// Normal transaction initiated with IStoreService.Purchase()/IStoreService.PurchaseAsync() call.
							_purchaseOp.TrySetException(new PurchaseException(product, reason));
						}
						else
						{
							// Should not really get here. A wierd transaction initiated directly with IStoreController.InitiatePurchase() call (bypassing IStoreService API).
							_console.TraceEvent(TraceEventType.Warning, _opId, $"{nameof(IStoreListener.OnPurchaseFailed)}: Unexpected product - got {productId} while {_productId} was expected.");
							_purchaseOp.TrySetException(new PurchaseException(product, reason));
						}
					}
					catch (Exception e)
					{
						// NOTE: Should never get here. Exception means logic error in the store implementation.
						_console.TraceData(TraceEventType.Critical, _opId, e);
						_purchaseOp?.TrySetException(new PurchaseException(product, reason, e));
					}
					finally
					{
						_opId = 0;
						_productId = null;
						_purchaseOp = null;
					}
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
				_initializeOp?.TrySetCanceled();
				_fetchOp?.TrySetCanceled();
				_purchaseOp?.TrySetCanceled();
			}
		}

		#endregion

		#region implementation

		private void OnFetch()
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, _opId, "OnFetch");
					_fetchOp.TrySetResult(null);
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _opId, e);
					_fetchOp.TrySetException(new StoreFetchException(InitializationFailureReason.PurchasingUnavailable, e));
				}
				finally
				{
					_opId = 0;
					_fetchOp = null;
				}
			}
		}

		private void OnFetchFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, _opId, $"OnFetchFailed: {error}.");
					_fetchOp.TrySetException(new StoreFetchException(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _opId, e);
					_fetchOp.TrySetException(new StoreFetchException(error, e));
				}
				finally
				{
					_opId = 0;
					_fetchOp = null;
				}
			}
		}

		private PurchaseProcessingResult ValidateProduct(Product product)
		{
			return PurchaseProcessingResult.Pending;
		}

		private void TraceUnexpectedProduct(string productId, string expectedProductId)
		{
			_console.TraceEvent(TraceEventType.Warning, 0, $"Unexpected product. Got {productId} while {expectedProductId} was expected.");
		}

		#endregion
	}
}
