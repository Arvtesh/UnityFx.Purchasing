// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreService"/>.
	/// </summary>
	internal sealed partial class StoreService : IStoreService, IStoreProductCollection, IObservable<PurchaseInfo>
	{
		#region data

		private const int _traceEventInitialize = 1;
		private const int _traceEventFetch = 2;
		private const int _traceEventPurchase = 3;

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly IStoreDelegate _delegate;
		private readonly IPurchasingModule _purchasingModule;

		private Dictionary<string, IStoreProduct> _products = new Dictionary<string, IStoreProduct>();
		private List<IObserver<PurchaseInfo>> _observers;
		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<object> _fetchOpCs;
		private TaskCompletionSource<PurchaseResult> _purchaseOpCs;
		private string _purchaseProductId;
		private IStoreProduct _purchaseProduct;
		private IStoreController _storeController;
		private bool _disposed;

		#endregion

		#region interface

		internal StoreService(string name, IPurchasingModule purchasingModule, IStoreDelegate storeDelegate)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName, SourceLevels.All);
			_delegate = storeDelegate;
			_purchasingModule = purchasingModule;
		}

		#endregion

		#region IStoreService

		public event EventHandler StoreInitialized;
		public event EventHandler<PurchaseInitializationFailed> StoreInitializationFailed;
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		public IObservable<PurchaseInfo> Purchases => this;

		public SourceSwitch TraceSwitch => _console.Switch;

		public TraceListenerCollection TraceListeners => _console.Listeners;

		public IStoreProductCollection Products => this;

		public IStoreController Controller => _storeController;

		public bool IsInitialized => _storeController != null;

		public bool IsBusy => _purchaseOpCs != null;

		public async Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				if (_initializeOpCs != null)
				{
					// Initialization is pending.
					await _initializeOpCs.Task;
				}
				else if (Application.isMobilePlatform || Application.isEditor)
				{
					_console.TraceEvent(TraceEventType.Start, _traceEventInitialize, "Initialize");

					try
					{
						_initializeOpCs = new TaskCompletionSource<object>();

						// 1) Get store configuration. Should be provided by the service user.
						var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
						var storeConfig = await _delegate.GetStoreConfigAsync();

						// 2) Initialize the store content.
						foreach (var product in storeConfig.Products)
						{
							var productDefinition = product.Definition;
							configurationBuilder.AddProduct(productDefinition.id, productDefinition.type);
							_products.Add(productDefinition.id, product);
						}

						// 3) Request the store data. This connects to real store and retrieves information on products specified in the previous step.
						UnityPurchasing.Initialize(this, configurationBuilder);
						await _initializeOpCs.Task;

						// 4) Trigger user-defined events.
						InvokeInitializeCompleted(_traceEventInitialize);
					}
					catch (StoreInitializeException e)
					{
						InvokeInitializeFailed(_traceEventInitialize, GetInitializeError(e.Reason), e);
						throw;
					}
					catch (Exception e)
					{
						_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
						InvokeInitializeFailed(_traceEventInitialize, StoreInitializeError.Unknown, e);
						throw;
					}
					finally
					{
						_initializeOpCs = null;
					}
				}
			}
		}

		public async Task FetchAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				await InitializeAsync();
			}
			else if (_fetchOpCs != null)
			{
				await _fetchOpCs.Task;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventFetch, "Fetch");

				try
				{
					_fetchOpCs = new TaskCompletionSource<object>();

					// 1) Get store configuration. Should be provided by the service user.
					var storeConfig = await _delegate.GetStoreConfigAsync();
					var productsToFetch = new HashSet<ProductDefinition>();

					// 2) Initialize the store content.
					foreach (var product in storeConfig.Products)
					{
						var productDefinition = product.Definition;

						if (_products.ContainsKey(productDefinition.id))
						{
							_products[productDefinition.id] = product;
						}
						else
						{
							_products.Add(productDefinition.id, product);
						}

						productsToFetch.Add(productDefinition);
					}

					// 3) Request the store data. This connects to real store and retrieves information on products specified in the previous step.
					_storeController.FetchAdditionalProducts(productsToFetch, OnFetch, OnFetchFailed);
					await _fetchOpCs.Task;

					// 4) Trigger user-defined events.
					InvokeInitializeCompleted(_traceEventFetch);
				}
				catch (StoreInitializeException e)
				{
					InvokeInitializeFailed(_traceEventFetch, GetInitializeError(e.Reason), e);
					throw;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventFetch, e);
					InvokeInitializeFailed(_traceEventFetch, StoreInitializeError.Unknown, e);
					throw;
				}
				finally
				{
					_fetchOpCs = null;
				}
			}
		}

		public async Task<PurchaseResult> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Turn on user-defined wait animation (if any).
			using (_delegate.BeginWait())
			{
				// 2) Notify user of the purchase.
				InvokePurchaseInitiated(productId, false);

				try
				{
					// 3) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 4) Wait for the fetch operation to complete (if any).
					if (_fetchOpCs != null)
					{
						await _fetchOpCs.Task;
					}

					// 5) Look up the Product reference with the general product identifier and the Purchasing system's products collection.
					var product = InitializeTransaction(productId);

					// 6) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
					if (product != null && product.availableToPurchase)
					{
						_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
						_purchaseOpCs = new TaskCompletionSource<PurchaseResult>(product);
						_storeController.InitiatePurchase(product);

						// 7) Wait for the purchase and validation process to complete, notify users and return.
						var purchaseResult = await _purchaseOpCs.Task;
						InvokePurchaseCompleted(purchaseResult);
						return purchaseResult;
					}
					else
					{
						throw new StorePurchaseException(new PurchaseResult(_purchaseProduct), StorePurchaseError.ProductUnavailable);
					}
				}
				catch (StorePurchaseException e)
				{
					InvokePurchaseFailed(e.Result, e.Reason, e);
					throw;
				}
				catch (StoreInitializeException e)
				{
					_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"{GetEventName(_traceEventPurchase)} error: {productId}, reason = {e.Message}");
					throw;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
					InvokePurchaseFailed(new PurchaseResult(_purchaseProduct), StorePurchaseError.Unknown, e);
					throw;
				}
				finally
				{
					ReleaseTransaction();
				}
			}
		}

		#endregion

		#region IReadOnlyCollection

		public IStoreProduct this[string productId]
		{
			get
			{
				ThrowIfInvalidProductId(productId);
				return _products[productId];
			}
		}

		public int Count => _products.Count;

		public bool ContainsKey(string productId)
		{
			ThrowIfInvalidProductId(productId);
			return _products.ContainsKey(productId);
		}

		public bool TryGetValue(string productId, out IStoreProduct product)
		{
			ThrowIfInvalidProductId(productId);
			return _products.TryGetValue(productId, out product);
		}

		#endregion

		#region IEnumerable

		public IEnumerator<IStoreProduct> GetEnumerator()
		{
			return _products.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _products.Values.GetEnumerator();
		}

		#endregion

		#region IObservable

		private class Subscription : IDisposable
		{
			private readonly List<IObserver<PurchaseInfo>> _observers;
			private readonly IObserver<PurchaseInfo> _observer;

			public Subscription(List<IObserver<PurchaseInfo>> observers, IObserver<PurchaseInfo> observer)
			{
				_observers = observers;
				_observer = observer;
			}

			public void Dispose()
			{
				lock (_observers)
				{
					_observers.Remove(_observer);
				}
			}
		}

		public IDisposable Subscribe(IObserver<PurchaseInfo> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			ThrowIfDisposed();

			if (_observers == null)
			{
				_observers = new List<IObserver<PurchaseInfo>>() { observer };
			}
			else
			{
				lock (_observers)
				{
					_observers.Add(observer);
				}
			}

			return new Subscription(_observers, observer);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				if (_initializeOpCs != null)
				{
					InvokeInitializeFailed(_traceEventInitialize, StoreInitializeError.StoreDisposed, null);
					_initializeOpCs = null;
				}

				if (_fetchOpCs != null)
				{
					InvokeInitializeFailed(_traceEventFetch, StoreInitializeError.StoreDisposed, null);
					_fetchOpCs = null;
				}

				if (_purchaseOpCs != null)
				{
					InvokePurchaseFailed(new PurchaseResult(_purchaseProduct), StorePurchaseError.StoreDisposed, null);
					ReleaseTransaction();
				}

				try
				{
					if (_observers != null)
					{
						lock (_observers)
						{
							foreach (var item in _observers)
							{
								item.OnCompleted();
							}

							_observers.Clear();
						}
					}
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, 0, e);
				}

				_console.TraceEvent(TraceEventType.Verbose, 0, "Disposed");
				_console.Close();
				_products.Clear();
				_storeController = null;
				_disposed = true;
			}
		}

		#endregion

		#region implementation

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(_serviceName);
			}
		}

		private void ThrowIfInvalidProductId(string productId)
		{
			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException(_serviceName + " product identifier cannot be null or empty string", nameof(productId));
			}
		}

		private void ThrowIfNotInitialized()
		{
			if (_storeController == null)
			{
				throw new InvalidOperationException(_serviceName + " is not initialized");
			}
		}

		private void ThrowIfBusy()
		{
			if (_purchaseOpCs != null || _purchaseProduct != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
