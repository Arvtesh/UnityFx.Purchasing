// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
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
	public abstract partial class StoreService : IStoreService, IStoreServiceSettings
	{
		#region data

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly IPurchasingModule _purchasingModule;
		private readonly StoreProductCollection _products;
		private readonly StoreListener _storeListener;
		private readonly StoreObservable _observer;

		private PurchaseOperation _purchaseOperation;

		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<object> _fetchOpCs;
		private IStoreController _storeController;
		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// Identifier for initialize-related trace events.
		/// </summary>
		protected const int TraceEventInitialize = (int)TraceEventId.Initialize;

		/// <summary>
		/// Identifier for fetch-related trace events.
		/// </summary>
		protected const int TraceEventFetch = (int)TraceEventId.Fetch;

		/// <summary>
		/// Identifier for purchase-related trace events.
		/// </summary>
		protected const int TraceEventPurchase = (int)TraceEventId.Purchase;

		/// <summary>
		/// Identifier for user trace events.
		/// </summary>
		protected const int TraceEventMax = 4;

		/// <summary>
		/// Returns the <see cref="System.Diagnostics.TraceSource"/> instance used by the service. Read only.
		/// </summary>
		protected TraceSource TraceSource => _console;

		/// <summary>
		/// Returns <c>true</c> if the service is disposed; <c>false</c> otherwise. Read only.
		/// </summary>
		protected bool IsDisposed => _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_products = new StoreProductCollection();
			_storeListener = new StoreListener(this);
			_observer = new StoreObservable();
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		protected abstract Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <c>null</c> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <param name="product">Reference to a product being purchased.</param>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		protected virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(IStoreProduct product, StoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}

		/// <summary>
		/// Called when the store initialization has succeeded.
		/// </summary>
		protected virtual void OnInitializeCompleted()
		{
			StoreInitialized?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when the store initialization has failed.
		/// </summary>
		protected virtual void OnInitializeFailed(StoreInitializeError reason, Exception e)
		{
			StoreInitializationFailed?.Invoke(this, new PurchaseInitializationFailed(reason, e));
		}

		/// <summary>
		/// Called when the store initialization has failed.
		/// </summary>
		protected virtual void OnPurchaseInitiated(string productId, bool isRestored)
		{
			PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, isRestored));
		}

		/// <summary>
		/// Called when the store purchase operation succeded.
		/// </summary>
		protected virtual void OnPurchaseCompleted(string productId, PurchaseResult purchaseResult)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
		}

		/// <summary>
		/// Called when the store purchase operation has failed.
		/// </summary>
		protected virtual void OnPurchaseFailed(string productId, PurchaseResult purchaseResult, StorePurchaseError reason, Exception e)
		{
			PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(productId, purchaseResult, reason, e));
		}

		/// <summary>
		/// Releases unmanaged resources used by the service.
		/// </summary>
		/// <param name="disposing">Should be <c>true</c> if the method is called from <see cref="Dispose()"/>; <c>false</c> otherwise.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				if (_initializeOpCs != null)
				{
					InvokeInitializeFailed(TraceEventInitialize, StoreInitializeError.StoreDisposed, null);
					_initializeOpCs = null;
				}

				if (_fetchOpCs != null)
				{
					InvokeInitializeFailed(TraceEventFetch, StoreInitializeError.StoreDisposed, null);
					_fetchOpCs = null;
				}

				if (_purchaseOperation != null)
				{
					InvokePurchaseFailed(_purchaseOperation.ProductId, new PurchaseResult(_purchaseOperation.Product), StorePurchaseError.StoreDisposed, null);
					_purchaseOperation.Dispose();
					_purchaseOperation = null;
				}

				try
				{
					_observer.OnCompleted();
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, 0, e);
				}

				_console.TraceEvent(TraceEventType.Verbose, 0, "Disposed");
				_console.Close();
				_products.Clear();
				_storeController = null;
			}
		}

		#endregion

		#region IStoreService

		/// <inheritdoc/>
		public event EventHandler StoreInitialized;

		/// <inheritdoc/>
		public event EventHandler<PurchaseInitializationFailed> StoreInitializationFailed;

		/// <inheritdoc/>
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <inheritdoc/>
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		/// <inheritdoc/>
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		/// <inheritdoc/>
		public IObservable<PurchaseInfo> Purchases => _observer;

		/// <inheritdoc/>
		public IStoreServiceSettings Settings => this;

		/// <inheritdoc/>
		public IStoreProductCollection Products => _products;

		/// <inheritdoc/>
		public IStoreController Controller => _storeController;

		/// <inheritdoc/>
		public bool IsInitialized => _storeController != null;

		/// <inheritdoc/>
		public bool IsBusy => _purchaseOperation != null;

		/// <inheritdoc/>
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
					_console.TraceEvent(TraceEventType.Start, TraceEventInitialize, "Initialize");

					try
					{
						_initializeOpCs = new TaskCompletionSource<object>();

						// 1) Get store configuration. Should be provided by the service user.
						var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
						var storeConfig = await GetStoreConfigAsync();

						// 2) Initialize the store content.
						foreach (var product in storeConfig.Products)
						{
							var productDefinition = product.Definition;
							configurationBuilder.AddProduct(productDefinition.id, productDefinition.type);
							_products.Add(productDefinition.id, product);
						}

						// 3) Request the store data. This connects to real store and retrieves information on products specified in the previous step.
						UnityPurchasing.Initialize(_storeListener, configurationBuilder);
						await _initializeOpCs.Task;
					}
					catch (StoreInitializeException e)
					{
						InvokeInitializeFailed(TraceEventInitialize, GetInitializeError(e.Reason), e);
						throw;
					}
					catch (Exception e)
					{
						_console.TraceData(TraceEventType.Error, TraceEventInitialize, e);
						InvokeInitializeFailed(TraceEventInitialize, StoreInitializeError.Unknown, e);
						throw;
					}
					finally
					{
						_initializeOpCs = null;
					}
				}
			}
		}

		/// <inheritdoc/>
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
				_console.TraceEvent(TraceEventType.Start, TraceEventFetch, "Fetch");

				try
				{
					_fetchOpCs = new TaskCompletionSource<object>();

					// 1) Get store configuration. Should be provided by the service user.
					var storeConfig = await GetStoreConfigAsync();
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
				}
				catch (StoreInitializeException e)
				{
					InvokeInitializeFailed(TraceEventFetch, GetInitializeError(e.Reason), e);
					throw;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, TraceEventFetch, e);
					InvokeInitializeFailed(TraceEventFetch, StoreInitializeError.Unknown, e);
					throw;
				}
				finally
				{
					_fetchOpCs = null;
				}
			}
		}

		/// <inheritdoc/>
		public async Task<PurchaseResult> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Initialize store transaction.
			using (var transaction = new PurchaseOperation(this, _console, productId, false))
			{
				_purchaseOperation = transaction;

				try
				{
					// 2) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 3) Wait for the fetch operation to complete (if any).
					if (_fetchOpCs != null)
					{
						await _fetchOpCs.Task;
					}

					// 4) Look up the Product reference with the product identifier and the Purchasing system's products collection.
					var product = transaction.Initialize();

					// 5) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
					if (product != null && product.availableToPurchase)
					{
						// 6) Wait for the purchase and validation process to complete, notify users and return.
						var purchaseResult = await transaction.Purchase(product);
						InvokePurchaseCompleted(productId, purchaseResult);
						return purchaseResult;
					}
					else
					{
						throw new StorePurchaseException(new PurchaseResult(transaction.Product), StorePurchaseError.ProductUnavailable);
					}
				}
				catch (StorePurchaseException e)
				{
					InvokePurchaseFailed(productId, e.Result, e.Reason, e);
					throw;
				}
				catch (StoreInitializeException e)
				{
					_console.TraceEvent(TraceEventType.Error, TraceEventPurchase, $"{GetEventName(TraceEventPurchase)} error: {productId}, reason = {e.Message}");
					throw;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
					InvokePurchaseFailed(productId, new PurchaseResult(transaction.Product), StorePurchaseError.Unknown, e);
					throw;
				}
				finally
				{
					_purchaseOperation = null;
				}
			}
		}

		#endregion

		#region IStoreServiceSettings

		/// <inheritdoc/>
		public SourceSwitch TraceSwitch { get => _console.Switch; set => _console.Switch = value; }

		/// <inheritdoc/>
		public TraceListenerCollection TraceListeners => _console.Listeners;

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			if (_purchaseOperation != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
