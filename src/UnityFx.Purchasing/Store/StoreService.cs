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
	using Debug = System.Diagnostics.Debug;

	/// <summary>
	/// Implementation of <see cref="IStoreService"/>.
	/// </summary>
	public abstract class StoreService : IStoreService, IStoreServiceSettings
	{
		#region data

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly IPurchasingModule _purchasingModule;
		private readonly StoreListener _storeListener;

		private StoreProductCollection _products;
		private StoreObservable<PurchaseResult> _purchases;
		private StoreObservable<FailedPurchaseResult> _failedPurchases;

		private IStoreController _storeController;
		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// Identifier for user trace events.
		/// </summary>
		protected const int TraceEventMax = 4;

		/// <summary>
		/// Returns the <see cref="System.Diagnostics.TraceSource"/> instance used by the service. Read only.
		/// </summary>
		protected TraceSource TraceSource => _console;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_storeListener = new StoreListener(this, _console);
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect app server for information on products available.
		/// </remarks>
		protected internal abstract Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <see langword="null"/> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) then initiate server-side validation.
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		protected internal virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(StoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}

		/// <summary>
		/// Called when the store initialize operation has been initiated.
		/// </summary>
		protected virtual void OnInitializeInitiated()
		{
			StoreFetchInitiated?.Invoke(this, new StoreFetchEventArgs(false));
		}

		/// <summary>
		/// Called when the store initialization has succeeded.
		/// </summary>
		protected virtual void OnInitializeCompleted(ProductCollection products)
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(false));
		}

		/// <summary>
		/// Called when the store initialization has failed.
		/// </summary>
		protected virtual void OnInitializeFailed(StoreFetchError reason, Exception e)
		{
			StoreFetchFailed?.Invoke(this, new StoreFetchFailedEventArgs(false, reason, e));
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated.
		/// </summary>
		protected virtual void OnFetchInitiated()
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(true));
		}

		/// <summary>
		/// Called when the store fetch has succeeded.
		/// </summary>
		protected virtual void OnFetchCompleted(ProductCollection products)
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(true));
		}

		/// <summary>
		/// Called when the store fetch has failed.
		/// </summary>
		protected virtual void OnFetchFailed(StoreFetchError reason, Exception e)
		{
			StoreFetchFailed?.Invoke(this, new StoreFetchFailedEventArgs(true, reason, e));
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated.
		/// </summary>
		protected virtual void OnPurchaseInitiated(string productId, bool isRestored)
		{
			PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, isRestored));
		}

		/// <summary>
		/// Called when the store purchase operation succeded.
		/// </summary>
		protected virtual void OnPurchaseCompleted(PurchaseResult purchaseResult)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
		}

		/// <summary>
		/// Called when the store purchase operation has failed.
		/// </summary>
		protected virtual void OnPurchaseFailed(FailedPurchaseResult purchaseResult)
		{
			PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(purchaseResult));
		}

		/// <summary>
		/// Releases unmanaged resources used by the service.
		/// </summary>
		/// <param name="disposing">Should be <see langword="true"/> if the method is called from <see cref="Dispose()"/>; <see langword="false"/> otherwise.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_products?.SetController(null);
				_storeListener.Dispose();

				try
				{
					_purchases?.OnCompleted();
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, 0, e);
				}

				try
				{
					_failedPurchases?.OnCompleted();
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, 0, e);
				}

				_console.TraceEvent(TraceEventType.Verbose, 0, "Disposed");
				_storeController = null;
			}
		}

		/// <summary>
		/// Throws an <see cref="ObjectDisposedException"/> if the instance is already disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(_serviceName);
			}
		}

		/// <summary>
		/// Throws an <see cref="ArgumentException"/> if the specified <paramref name="productId"/> is <see langword="null"/> or empty string.
		/// </summary>
		protected void ThrowIfInvalidProductId(string productId)
		{
			if (productId == null)
			{
				throw new ArgumentNullException(_serviceName + " product identifier cannot be null", nameof(productId));
			}

			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException(_serviceName + " product identifier cannot be an empty string", nameof(productId));
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the service is not initialized yet.
		/// </summary>
		protected void ThrowIfNotInitialized()
		{
			if (_storeController == null)
			{
				throw new InvalidOperationException(_serviceName + " is not initialized");
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if a purchase operation is currently running.
		/// </summary>
		protected void ThrowIfBusy()
		{
			if (_storeListener.IsPurchasePending)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion

		#region internals

		internal enum TraceEventId
		{
			Default,
			Initialize,
			Fetch,
			Purchase
		}

		internal void SetStoreController(IStoreController controller)
		{
			_storeController = controller;
			_products?.SetController(controller);
		}

		internal void InvokeInitializeInitiated()
		{
			try
			{
				OnInitializeInitiated();
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
			}
		}

		internal void InvokeInitializeCompleted(ProductCollection products)
		{
			try
			{
				OnInitializeCompleted(products);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
			}
		}

		internal void InvokeInitializeFailed(StoreFetchError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, (int)TraceEventId.Initialize, TraceEventId.Initialize.ToString() + " error: " + reason);

			try
			{
				OnInitializeFailed(reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
			}
		}

		internal void InvokeFetchInitiated()
		{
			try
			{
				OnFetchInitiated();
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
			}
		}

		internal void InvokeFetchCompleted(ProductCollection products)
		{
			try
			{
				OnInitializeCompleted(products);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
			}
		}

		internal void InvokeFetchFailed(StoreFetchError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, (int)TraceEventId.Fetch, TraceEventId.Fetch.ToString() + " error: " + reason);

			try
			{
				OnInitializeFailed(reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
			}
		}

		internal void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			try
			{
				OnPurchaseInitiated(productId, restored);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}
		}

		internal void InvokePurchaseCompleted(string productId, PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			try
			{
				_purchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}

			try
			{
				OnPurchaseCompleted(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}
		}

		internal void InvokePurchaseFailed(FailedPurchaseResult purchaseResult)
		{
			_console.TraceEvent(TraceEventType.Error, (int)TraceEventId.Purchase, $"{TraceEventId.Purchase.ToString()} error: {purchaseResult.ProductId}, reason = {purchaseResult.Error}");

			try
			{
				_failedPurchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}

			try
			{
				OnPurchaseFailed(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}
		}

		internal static StoreFetchError GetInitializeError(InitializationFailureReason error)
		{
			switch (error)
			{
				case InitializationFailureReason.AppNotKnown:
					return StoreFetchError.AppNotKnown;

				case InitializationFailureReason.NoProductsAvailable:
					return StoreFetchError.NoProductsAvailable;

				case InitializationFailureReason.PurchasingUnavailable:
					return StoreFetchError.PurchasingUnavailable;

				default:
					return StoreFetchError.Unknown;
			}
		}

		internal static StorePurchaseError GetPurchaseError(PurchaseFailureReason error)
		{
			switch (error)
			{
				case PurchaseFailureReason.PurchasingUnavailable:
					return StorePurchaseError.PurchasingUnavailable;

				case PurchaseFailureReason.ExistingPurchasePending:
					return StorePurchaseError.ExistingPurchasePending;

				case PurchaseFailureReason.ProductUnavailable:
					return StorePurchaseError.ProductUnavailable;

				case PurchaseFailureReason.SignatureInvalid:
					return StorePurchaseError.SignatureInvalid;

				case PurchaseFailureReason.UserCancelled:
					return StorePurchaseError.UserCanceled;

				case PurchaseFailureReason.PaymentDeclined:
					return StorePurchaseError.PaymentDeclined;

				case PurchaseFailureReason.DuplicateTransaction:
					return StorePurchaseError.DuplicateTransaction;

				default:
					return StorePurchaseError.Unknown;
			}
		}

		#endregion

		#region IStoreService

		/// <inheritdoc/>
		public IStoreServiceSettings Settings
		{
			get
			{
				ThrowIfDisposed();
				return this;
			}
		}

		/// <inheritdoc/>
		public IStoreProductCollection Products
		{
			get
			{
				ThrowIfDisposed();

				if (_products == null)
				{
					_products = new StoreProductCollection(_storeController);
				}

				return _products;
			}
		}

		/// <inheritdoc/>
		public IStoreController Controller
		{
			get
			{
				ThrowIfDisposed();
				return _storeController;
			}
		}

		/// <inheritdoc/>
		public bool IsInitialized
		{
			get
			{
				ThrowIfDisposed();
				return _storeController != null;
			}
		}

		/// <inheritdoc/>
		public bool IsBusy
		{
			get
			{
				ThrowIfDisposed();
				return _storeListener.IsPurchasePending;
			}
		}

		/// <inheritdoc/>
		public async Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				if (_storeListener.IsInitializePending)
				{
					await _storeListener.InitializeTask;
				}
				else if (Application.isMobilePlatform || Application.isEditor)
				{
					using (var op = _storeListener.BeginInitialize())
					{
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
							InvokeInitializeFailed(StoreFetchError.Unknown, e);
							throw;
						}
						finally
						{
							_storeListener.EndInitialize();
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
			else if (_storeListener.IsFetchPending)
			{
				await _storeListener.FetchTask;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				using (var op = _storeListener.BeginFetch())
				{
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
						InvokeFetchFailed(StoreFetchError.Unknown, e);
						throw;
					}
					finally
					{
						_storeListener.EndFetch();
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
			using (var op = _storeListener.BeginPurchase(productId, false))
			{
				try
				{
					// 2) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 3) Wait for the fetch operation to complete (if any).
					if (_storeListener.IsFetchPending)
					{
						await _storeListener.FetchTask;
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
					InvokePurchaseFailed(new FailedPurchaseResult(productId, e));
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
					InvokePurchaseFailed(new FailedPurchaseResult(productId, null, null, StorePurchaseError.Unknown, e));
					throw;
				}
				finally
				{
					_storeListener.EndPurchase();
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

		#region IStoreEvents

		/// <inheritdoc/>
		public event EventHandler<StoreFetchEventArgs> StoreFetchInitiated;

		/// <inheritdoc/>
		public event EventHandler<StoreFetchEventArgs> StoreFetchCompleted;

		/// <inheritdoc/>
		public event EventHandler<StoreFetchFailedEventArgs> StoreFetchFailed;

		/// <inheritdoc/>
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <inheritdoc/>
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		/// <inheritdoc/>
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		/// <inheritdoc/>
		public IObservable<PurchaseResult> Purchases
		{
			get
			{
				ThrowIfDisposed();

				if (_purchases == null)
				{
					_purchases = new StoreObservable<PurchaseResult>();
				}

				return _purchases;
			}
		}

		/// <inheritdoc/>
		public IObservable<FailedPurchaseResult> FailedPurchases
		{
			get
			{
				ThrowIfDisposed();

				if (_failedPurchases == null)
				{
					_failedPurchases = new StoreObservable<FailedPurchaseResult>();
				}

				return _failedPurchases;
			}
		}

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
		#endregion
	}
}
