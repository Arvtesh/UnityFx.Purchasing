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

#if !NET35
		private StoreObservable<PurchaseResult> _purchases;
		private StoreObservable<FailedPurchaseResult> _failedPurchases;
#endif

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
		/// <param name="onSuccess">Operation completed delegate.</param>
		/// <param name="onFailure">Delegate called on operation failure.</param>
		/// <seealso cref="ValidatePurchase(StoreTransaction, Action{PurchaseValidationResult}, Action{Exception})"/>
		protected internal abstract void GetStoreConfig(Action<StoreConfig> onSuccess, Action<Exception> onFailure);

#if !NET35
		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect app server for information on products available.
		/// </remarks>
		/// <seealso cref="ValidatePurchaseAsync(StoreTransaction)"/>
		protected internal abstract Task<StoreConfig> GetStoreConfigAsync();
#endif

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <see langword="null"/> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) then initiate server-side validation.
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <param name="onSuccess">Operation completed delegate.</param>
		/// <param name="onFailure">Delegate called on operation failure.</param>
		/// <seealso cref="GetStoreConfig(Action{StoreConfig}, Action{Exception})"/>
		protected internal virtual bool ValidatePurchase(StoreTransaction transactionInfo, Action<PurchaseValidationResult> onSuccess, Action<Exception> onFailure)
		{
			return false;
		}

#if !NET35
		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <see langword="null"/> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) then initiate server-side validation.
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <seealso cref="GetStoreConfigAsync"/>
		protected internal virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(StoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}
#endif

		/// <summary>
		/// Called when the store initialize operation has been initiated.
		/// </summary>
		/// <seealso cref="OnInitializeCompleted()"/>
		/// <seealso cref="OnInitializeFailed(StoreFetchError, Exception)"/>
		protected virtual void OnInitializeInitiated()
		{
			StoreFetchInitiated?.Invoke(this, new StoreFetchEventArgs(false));
		}

		/// <summary>
		/// Called when the store initialization has succeeded.
		/// </summary>
		/// <seealso cref="OnInitializeFailed(StoreFetchError, Exception)"/>
		/// <seealso cref="OnInitializeInitiated"/>
		protected virtual void OnInitializeCompleted()
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(false));
		}

		/// <summary>
		/// Called when the store initialization has failed.
		/// </summary>
		/// <seealso cref="OnInitializeCompleted()"/>
		/// <seealso cref="OnInitializeInitiated"/>
		protected virtual void OnInitializeFailed(StoreFetchError reason, Exception e)
		{
			StoreFetchFailed?.Invoke(this, new StoreFetchFailedEventArgs(false, reason, e));
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated.
		/// </summary>
		/// <seealso cref="OnFetchCompleted()"/>
		/// <seealso cref="OnFetchFailed(StoreFetchError, Exception)"/>
		protected virtual void OnFetchInitiated()
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(true));
		}

		/// <summary>
		/// Called when the store fetch has succeeded.
		/// </summary>
		/// <seealso cref="OnFetchFailed(StoreFetchError, Exception)"/>
		/// <seealso cref="OnFetchInitiated"/>
		protected virtual void OnFetchCompleted()
		{
			StoreFetchCompleted?.Invoke(this, new StoreFetchEventArgs(true));
		}

		/// <summary>
		/// Called when the store fetch has failed.
		/// </summary>
		/// <seealso cref="OnFetchCompleted()"/>
		/// <seealso cref="OnFetchInitiated"/>
		protected virtual void OnFetchFailed(StoreFetchError reason, Exception e)
		{
			StoreFetchFailed?.Invoke(this, new StoreFetchFailedEventArgs(true, reason, e));
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated.
		/// </summary>
		/// <seealso cref="OnPurchaseCompleted(PurchaseResult)"/>
		/// <seealso cref="OnPurchaseFailed(FailedPurchaseResult)"/>
		protected virtual void OnPurchaseInitiated(string productId, bool isRestored)
		{
			PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, isRestored));
		}

		/// <summary>
		/// Called when the store purchase operation succeded.
		/// </summary>
		/// <seealso cref="OnPurchaseFailed(FailedPurchaseResult)"/>
		/// <seealso cref="OnPurchaseInitiated(string, bool)"/>
		protected virtual void OnPurchaseCompleted(PurchaseResult purchaseResult)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
		}

		/// <summary>
		/// Called when the store purchase operation has failed.
		/// </summary>
		/// <seealso cref="OnPurchaseCompleted(PurchaseResult)"/>
		/// <seealso cref="OnPurchaseInitiated(string, bool)"/>
		protected virtual void OnPurchaseFailed(FailedPurchaseResult purchaseResult)
		{
			PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(purchaseResult));
		}

		/// <summary>
		/// Releases unmanaged resources used by the service.
		/// </summary>
		/// <param name="disposing">Should be <see langword="true"/> if the method is called from <see cref="Dispose()"/>; <see langword="false"/> otherwise.</param>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="ThrowIfDisposed"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_storeListener.Dispose();

				SetStoreController(null);

#if !NET35
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
#endif

				_console.TraceEvent(TraceEventType.Verbose, 0, "Disposed");
			}
		}

		/// <summary>
		/// Throws an <see cref="ObjectDisposedException"/> if the instance is already disposed.
		/// </summary>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="Dispose(bool)"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		/// <seealso cref="ThrowIfBusy"/>
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
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		/// <seealso cref="ThrowIfBusy"/>
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
		/// <seealso cref="IsInitialized"/>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfBusy"/>
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
		/// <seealso cref="IsBusy"/>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		protected void ThrowIfBusy()
		{
			if (_storeListener.IsPurchasePending)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion

		#region internals

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

		internal void InvokeInitializeCompleted()
		{
			try
			{
				OnInitializeCompleted();
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

		internal void InvokeFetchCompleted()
		{
			try
			{
				OnInitializeCompleted();
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

#if !NET35
			try
			{
				_purchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}
#endif

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

#if !NET35
			try
			{
				_failedPurchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}
#endif

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
		public AsyncResult Initialize()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				if (_storeListener.IsInitializePending)
				{
					return _storeListener.InitializeAsyncResult;
				}
				else if (Application.isMobilePlatform || Application.isEditor)
				{
					var op = _storeListener.BeginInitialize();

					try
					{
						GetStoreConfig(InitializeGetConfigCallback, InitializeGetConfigErrorCallback);
					}
					catch (Exception e)
					{
						_storeListener.EndInitialize(e);
						throw;
					}

					// TODO: use op.??? here
					return _storeListener.InitializeAsyncResult;
				}
				else
				{
					throw new PlatformNotSupportedException();
				}
			}

			return AsyncResult.Completed;
		}

#if !NET35
		/// <inheritdoc/>
		public Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				if (_storeListener.IsInitializePending)
				{
					return _storeListener.InitializeTask;
				}
				else if (Application.isMobilePlatform || Application.isEditor)
				{
					var op = _storeListener.BeginInitialize();

					try
					{
						GetStoreConfigAsync().ContinueWith(task =>
						{
							if (task.Status == TaskStatus.RanToCompletion)
							{
								InitializeGetConfigCallback(task.Result);
							}
							else
							{
								InitializeGetConfigErrorCallback(task.Exception);
							}
						});
					}
					catch (Exception e)
					{
						_storeListener.EndInitialize(e);
						throw;
					}

					return op.Task;
				}
				else
				{
					throw new PlatformNotSupportedException();
				}
			}

			return Task.CompletedTask;
		}
#endif

		/// <inheritdoc/>
		public AsyncResult Fetch()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				return Initialize();
			}
			else if (_storeListener.IsFetchPending)
			{
				return _storeListener.FetchAsyncResult;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				var op = _storeListener.BeginFetch();

				try
				{
					GetStoreConfig(FetchGetConfigCallback, FetchGetConfigErrorCallback);
				}
				catch (Exception e)
				{
					_storeListener.EndFetch(e);
					throw;
				}

				// TODO: use op.??? here
				return _storeListener.FetchAsyncResult;
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

#if !NET35
		/// <inheritdoc/>
		public Task FetchAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				return InitializeAsync();
			}
			else if (_storeListener.IsFetchPending)
			{
				return _storeListener.FetchTask;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				var op = _storeListener.BeginFetch();

				try
				{
					GetStoreConfigAsync().ContinueWith(task =>
					{
						if (task.Status == TaskStatus.RanToCompletion)
						{
							FetchGetConfigCallback(task.Result);
						}
						else
						{
							FetchGetConfigErrorCallback(task.Exception);
						}
					});
				}
				catch (Exception e)
				{
					_storeListener.EndFetch(e);
					throw;
				}

				return op.Task;
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}
#endif

		/// <inheritdoc/>
		public AsyncResult<PurchaseResult> Purchase(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Initialize store transaction.
			var op = _storeListener.BeginPurchase(productId, false);

			try
			{
				// 2) Wait untill the store initialization is finished.
				if (_storeController == null)
				{
					// TODO
				}

				// 3) Wait for the fetch operation to complete (if any).
				if (_storeListener.IsFetchPending)
				{
					// TODO
				}

				// 4) Look up the Product reference with the product identifier and the Purchasing system's products collection.
				var product = _storeController.products.WithID(productId);

				// 5) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
				if (product != null && product.availableToPurchase)
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"InitiatePurchase: {productId} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
					_storeController.InitiatePurchase(product);

					// TODO: use op.??? here
					return _storeListener.PurchaseAsyncResult;
				}
				else
				{
					throw new StorePurchaseException(new PurchaseResult(product), StorePurchaseError.ProductUnavailable);
				}
			}
			catch (Exception e)
			{
				_storeListener.EndPurchase(e);
				throw;
			}
		}

#if !NET35
		/// <inheritdoc/>
		public async Task<PurchaseResult> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Initialize store transaction.
			var op = _storeListener.BeginPurchase(productId, false);

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
#endif

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

#if !NET35
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
#endif

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

		private void InitializeGetConfigCallback(StoreConfig storeConfig)
		{
			if (!_disposed)
			{
				if (storeConfig != null)
				{
					if (storeConfig.Products != null)
					{
						var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
						configurationBuilder.AddProducts(storeConfig.Products);

						UnityPurchasing.Initialize(_storeListener, configurationBuilder);
					}
					else
					{
						_storeListener.EndInitialize(new ArgumentNullException(nameof(storeConfig.Products)));
					}
				}
				else
				{
					_storeListener.EndInitialize(new ArgumentNullException(nameof(storeConfig)));
				}
			}
		}

		private void InitializeGetConfigErrorCallback(Exception e)
		{
			if (!_disposed)
			{
				_storeListener.EndInitialize(e);
			}
		}

		private void FetchGetConfigCallback(StoreConfig storeConfig)
		{
			if (!_disposed)
			{
				if (storeConfig != null)
				{
					if (storeConfig.Products != null)
					{
						var products = new HashSet<ProductDefinition>(storeConfig.Products);
						_storeController.FetchAdditionalProducts(products, _storeListener.OnFetch, _storeListener.OnFetchFailed);
					}
					else
					{
						_storeListener.EndFetch(new ArgumentNullException(nameof(storeConfig.Products)));
					}
				}
				else
				{
					_storeListener.EndFetch(new ArgumentNullException(nameof(storeConfig)));
				}
			}
		}

		private void FetchGetConfigErrorCallback(Exception e)
		{
			if (!_disposed)
			{
				_storeListener.EndFetch(e);
			}
		}

		#endregion
	}
}
