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

		private InitializeOperation _initializeOp;
		private FetchOperation _fetchOp;
		private PurchaseOperation _purchaseOp;

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
		public bool IsDisposed => _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_products = new StoreProductCollection();
			_storeListener = new StoreListener(this, _console);
			_observer = new StoreObservable();
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		protected abstract Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <c>null</c> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		protected virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(StoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}

		/// <summary>
		/// Called when the store initialization has succeeded.
		/// </summary>
		protected virtual void OnInitializeCompleted(ProductCollection products)
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
		/// Called when the store fetch has succeeded.
		/// </summary>
		protected virtual void OnFetchInitiated()
		{
		}

		/// <summary>
		/// Called when the store fetch has succeeded.
		/// </summary>
		protected virtual void OnFetchCompleted(ProductCollection products)
		{
		}

		/// <summary>
		/// Called when the store fetch has failed.
		/// </summary>
		protected virtual void OnFetchFailed(StoreInitializeError reason, Exception e)
		{
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

				if (_initializeOp != null)
				{
					InvokeInitializeFailed(StoreInitializeError.StoreDisposed, null);
					_initializeOp.Dispose();
					_initializeOp = null;
				}

				if (_fetchOp != null)
				{
					InvokeFetchFailed(StoreInitializeError.StoreDisposed, null);
					_fetchOp.Dispose();
					_fetchOp = null;
				}

				if (_purchaseOp != null)
				{
					InvokePurchaseFailed(_purchaseOp.ProductId, new PurchaseResult(null), StorePurchaseError.StoreDisposed, null);
					_purchaseOp.Dispose();
					_purchaseOp = null;
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
		public bool IsBusy => _purchaseOp != null;

		/// <inheritdoc/>
		public async Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				if (_initializeOp != null)
				{
					await _initializeOp.Task;
				}
				else if (Application.isMobilePlatform || Application.isEditor)
				{
					using (var op = new InitializeOperation(_console))
					{
						_initializeOp = op;
						_storeListener.SetInitializeOp(op);

						try
						{
							// Get user-defined store config.
							var storeConfig = await GetStoreConfigAsync();

							// Configure Unity store.
							var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
							configurationBuilder.AddProducts(storeConfig.Products);

							// Initialize Unity store.
							UnityPurchasing.Initialize(_storeListener, configurationBuilder);
							await op.Task;

							// Notify subscribers of the operation success.
							InvokeInitializeCompleted(_storeController.products);
						}
						catch (StoreFetchException e)
						{
							InvokeInitializeFailed(GetInitializeError(e.Reason), e);
							throw;
						}
						catch (Exception e)
						{
							_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
							InvokeInitializeFailed(StoreInitializeError.Unknown, e);
							throw;
						}
						finally
						{
							_storeListener.SetInitializeOp(null);
							_initializeOp = null;
						}
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
			else if (_fetchOp != null)
			{
				await _fetchOp.Task;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				using (var op = new FetchOperation(_console))
				{
					_fetchOp = op;
					_storeListener.SetFetchOp(op);

					try
					{
						// Get user-defined store config.
						var storeConfig = await GetStoreConfigAsync();

						// Fetch Unity store.
						var products = new HashSet<ProductDefinition>(storeConfig.Products);
						_storeController.FetchAdditionalProducts(products, _storeListener.OnFetch, _storeListener.OnFetchFailed);
						await op.Task;

						// Notify subscribers of the operation success.
						InvokeFetchCompleted(_storeController.products);
					}
					catch (StoreFetchException e)
					{
						InvokeFetchFailed(GetInitializeError(e.Reason), e);
						throw;
					}
					catch (Exception e)
					{
						_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
						InvokeFetchFailed(StoreInitializeError.Unknown, e);
						throw;
					}
					finally
					{
						_storeListener.SetFetchOp(null);
						_fetchOp = null;
					}
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
			using (var op = new PurchaseOperation(this, _console, productId, false))
			{
				_purchaseOp = op;
				_storeListener.SetPurchaseOp(op);

				try
				{
					// 2) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 3) Wait for the fetch operation to complete (if any).
					if (_fetchOp != null)
					{
						await _fetchOp.Task;
					}

					// 4) Look up the Product reference with the product identifier and the Purchasing system's products collection.
					var product = _storeController.products.WithID(productId);

					// 5) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
					if (product != null && product.availableToPurchase)
					{
						_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"InitiatePurchase: {productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
						_storeController.InitiatePurchase(product);

						// 6) Wait for the purchase and validation process to complete, notify users and return.
						var purchaseResult = await op.Task;
						InvokePurchaseCompleted(productId, purchaseResult);
						return purchaseResult;
					}
					else
					{
						throw new StorePurchaseException(new PurchaseResult(product), StorePurchaseError.ProductUnavailable);
					}
				}
				catch (StorePurchaseException e)
				{
					InvokePurchaseFailed(productId, e.Result, e.Reason, e);
					throw;
				}
				catch (StoreFetchException e)
				{
					_console.TraceEvent(TraceEventType.Error, (int)TraceEventId.Purchase, $"{TraceEventId.Purchase.ToString()} error: {productId}, reason = {e.Message}");
					throw;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					InvokePurchaseFailed(productId, new PurchaseResult(null), StorePurchaseError.Unknown, e);
					throw;
				}
				finally
				{
					_storeListener.SetPurchaseOp(null);
					_purchaseOp = null;
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
			if (_purchaseOp != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
