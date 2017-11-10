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
	/// Implementation of <see cref="IStoreService{TProduct}"/>.
	/// </summary>
	public abstract class StoreService<TProduct> : IStoreService<TProduct> where TProduct : class, IStoreProduct
	{
		#region data

		private const int _traceEventInitialize = 1;
		private const int _traceEventFetch = 2;
		private const int _traceEventPurchase = 3;

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly IPurchasingModule _purchasingModule;

		private StoreProductCollection<TProduct> _products;
		private StoreListener _storeListener;
		private StoreObservable _observer;

		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<object> _fetchOpCs;
		private TaskCompletionSource<PurchaseResult> _purchaseOpCs;
		private string _purchaseProductId;
		private TProduct _purchaseProduct;
		private IStoreController _storeController;

		#endregion

		#region interface

		/// <summary>
		/// Returns the <see cref="TraceSource"/> instance used by the service. Read only.
		/// </summary>
		protected TraceSource Console => _console;

		/// <summary>
		/// Returns <c>true</c> if the service is disposed; <c>false</c> otherwise. Read only.
		/// </summary>
		protected bool IsDisposed => _storeListener != null;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService{TProduct}"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName, SourceLevels.All);
			_purchasingModule = purchasingModule;
			_storeListener = new StoreListener(this);
			_products = new StoreProductCollection<TProduct>();
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		protected abstract Task<StoreConfig<TProduct>> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <c>null</c> result value to indicate that no validation is needed (default behaviour).
		/// </summary>
		/// <param name="product">Reference to a product being purchased.</param>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		protected virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(TProduct product, StoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}

		/// <summary>
		/// Releases unmanaged resources used by the service.
		/// </summary>
		/// <param name="disposing">Should be <c>true</c> if the method is called from <see cref="Dispose()"/>; <c>false</c> otherwise.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _storeListener != null)
			{
				_storeListener = null;

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
					_observer?.OnCompleted();
					_observer = null;
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
		public IObservable<PurchaseInfo> Purchases
		{
			get
			{
				if (_observer == null)
				{
					_observer = new StoreObservable();
				}

				return _observer;
			}
		}

		/// <inheritdoc/>
		public IStoreProductCollection<TProduct> Products => _products;

		/// <inheritdoc/>
		public IStoreController Controller => _storeController;

		/// <inheritdoc/>
		public bool IsInitialized => _storeController != null;

		/// <inheritdoc/>
		public bool IsBusy => _purchaseOpCs != null;

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
					_console.TraceEvent(TraceEventType.Start, _traceEventInitialize, "Initialize");

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
				_console.TraceEvent(TraceEventType.Start, _traceEventFetch, "Fetch");

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

		/// <inheritdoc/>
		public async Task<PurchaseResult> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Notify user of the purchase.
			InvokePurchaseInitiated(productId, false);

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

				// 4) Look up the Product reference with the general product identifier and the Purchasing system's products collection.
				var product = InitializeTransaction(productId);

				// 5) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
				if (product != null && product.availableToPurchase)
				{
					_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
					_purchaseOpCs = new TaskCompletionSource<PurchaseResult>(product);
					_storeController.InitiatePurchase(product);

					// 6) Wait for the purchase and validation process to complete, notify users and return.
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

		#endregion

		#region IStoreListener

		private class StoreListener : IStoreListener
		{
			private StoreService<TProduct> _parentStore;

			public StoreListener(StoreService<TProduct> service)
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

		internal void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventInitialize, "OnInitialized");

			try
			{
				foreach (var product in controller.products.all)
				{
					if (_products.TryGetValue(product.definition.id, out var userProduct))
					{
						userProduct.Metadata = product.metadata;
					}
				}

				_storeController = controller;
				_initializeOpCs.SetResult(null);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
				_initializeOpCs.SetException(e);
			}
		}

		internal void OnInitializeFailed(InitializationFailureReason error)
		{
			_console.TraceEvent(TraceEventType.Verbose, _traceEventInitialize, "OnInitializeFailed: " + error);

			try
			{
				_initializeOpCs.SetException(new StoreInitializeException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
			}
		}

		internal PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			var product = args.purchasedProduct;
			var productId = product.definition.id;
			var isRestored = _purchaseOpCs == null;

			try
			{
				// If the purchase operation has been auto-restored _purchaseOpCs would be null.
				if (isRestored)
				{
					InvokePurchaseInitiated(productId, true);
					InitializeTransaction(productId);
				}

				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, "ProcessPurchase: " + productId);
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

				// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
				// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
				if (isRestored || _purchaseOpCs.Task.AsyncState.Equals(product))
				{
					var transactionInfo = new StoreTransaction(product, isRestored);

					if (string.IsNullOrEmpty(transactionInfo.Receipt))
					{
						SetPurchaseFailed(_purchaseProduct, transactionInfo, null, StorePurchaseError.ReceiptNullOrEmpty);
					}
					else
					{
						ValidatePurchase(_purchaseProduct, transactionInfo);
						return PurchaseProcessingResult.Pending;
					}
				}
			}
			catch (Exception e)
			{
				SetPurchaseFailed(_purchaseProduct, new StoreTransaction(product, isRestored), null, StorePurchaseError.Unknown, e);
			}

			return PurchaseProcessingResult.Complete;
		}

		internal void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var productId = product?.definition.id ?? "null";
			var isRestored = _purchaseOpCs == null;

			try
			{
				// If the purchase operation has been auto-restored, _purchaseOpCs would be null.
				if (isRestored)
				{
					InvokePurchaseInitiated(productId, true);
					InitializeTransaction(productId);
				}

				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"OnPurchaseFailed: {productId}, reason={failReason}");

				SetPurchaseFailed(_purchaseProduct, new StoreTransaction(product, isRestored), null, GetPurchaseError(failReason), null);
			}
			catch (Exception e)
			{
				SetPurchaseFailed(_purchaseProduct, new StoreTransaction(product, isRestored), null, GetPurchaseError(failReason), e);
			}
		}

		private void OnFetch()
		{
			// Quick return if the store has been disposed.
			if (IsDisposed)
			{
				return;
			}

			_console.TraceEvent(TraceEventType.Verbose, _traceEventFetch, "OnFetch");

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
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventFetch, e);
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

			_console.TraceEvent(TraceEventType.Verbose, _traceEventFetch, "OnFetchFailed: " + error);

			try
			{
				_fetchOpCs.SetException(new StoreInitializeException(error));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventFetch, e);
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

		private static StoreInitializeError GetInitializeError(InitializationFailureReason error)
		{
			switch (error)
			{
				case InitializationFailureReason.AppNotKnown:
					return StoreInitializeError.AppNotKnown;

				case InitializationFailureReason.NoProductsAvailable:
					return StoreInitializeError.NoProductsAvailable;

				case InitializationFailureReason.PurchasingUnavailable:
					return StoreInitializeError.PurchasingUnavailable;

				default:
					return StoreInitializeError.Unknown;
			}
		}

		private static StorePurchaseError GetPurchaseError(PurchaseFailureReason error)
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

		private async void ValidatePurchase(TProduct userProduct, StoreTransaction transactionInfo)
		{
			var product = transactionInfo.Product;
			var resultStatus = PurchaseValidationStatus.Failure;

			try
			{
				_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId = {product.transactionID}");

				var validationResult = await ValidatePurchaseAsync(userProduct, transactionInfo);

				if (!IsDisposed)
				{
					if (validationResult == null)
					{
						// No result returned from the validator means validation succeeded.
						ConfirmPendingPurchase(product);
						SetPurchaseCompleted(_purchaseProduct, transactionInfo, validationResult);
					}
					else
					{
						resultStatus = validationResult.Status;

						if (resultStatus == PurchaseValidationStatus.Ok)
						{
							// The purchase validation succeeded.
							ConfirmPendingPurchase(product);
							SetPurchaseCompleted(_purchaseProduct, transactionInfo, validationResult);
						}
						else if (resultStatus == PurchaseValidationStatus.Failure)
						{
							// The purchase validation failed: confirm to avoid processing it again.
							ConfirmPendingPurchase(product);
							SetPurchaseFailed(_purchaseProduct, transactionInfo, validationResult, StorePurchaseError.ReceiptValidationFailed);
						}
						else
						{
							// Need to re-validate the purchase: do not confirm.
							SetPurchaseFailed(_purchaseProduct, transactionInfo, validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
						}
					}
				}
			}
			catch (Exception e)
			{
				// NOTE: Should not really get here (do we need to confirm it in this case?).
				ConfirmPendingPurchase(product);
				SetPurchaseFailed(_purchaseProduct, transactionInfo, null, StorePurchaseError.ReceiptValidationFailed, e);
			}
		}

		private void SetPurchaseCompleted(TProduct product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			var result = new PurchaseResult(product, transactionInfo, validationResult);

			if (_purchaseOpCs != null)
			{
				_purchaseOpCs.SetResult(result);
			}
			else
			{
				InvokePurchaseCompleted(result);
				ReleaseTransaction();
			}
		}

		private void SetPurchaseFailed(TProduct product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e = null)
		{
			var result = new PurchaseResult(product, transactionInfo, validationResult);

			if (_purchaseOpCs != null)
			{
				if (failReason == StorePurchaseError.UserCanceled)
				{
					_purchaseOpCs.SetCanceled();
				}
				else if (e != null)
				{
					_purchaseOpCs.SetException(new StorePurchaseException(result, failReason, e));
				}
				else
				{
					_purchaseOpCs.SetException(new StorePurchaseException(result, failReason));
				}
			}
			else
			{
				InvokePurchaseFailed(result, failReason, e);
				ReleaseTransaction();
			}
		}

		private void InvokeInitializeCompleted(int opId)
		{
			try
			{
				StoreInitialized?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, opId, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, opId, GetEventName(opId) + " complete");
			}
		}

		private void InvokeInitializeFailed(int opId, StoreInitializeError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, opId, GetEventName(opId) + " error: " + reason);

			try
			{
				StoreInitializationFailed?.Invoke(this, new PurchaseInitializationFailed(reason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, opId, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, opId, GetEventName(opId) + " failed");
			}
		}

		private void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			if (restored)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, GetEventName(_traceEventPurchase) + " (auto-restored): " + productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventPurchase, GetEventName(_traceEventPurchase) + ": " + productId);
			}

			_purchaseProductId = productId;

			try
			{
				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, restored));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
		}

		private void InvokePurchaseCompleted(PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			if (_observer != null)
			{
				try
				{
					_observer.OnNext(new PurchaseInfo(_purchaseProductId, purchaseResult, null, null));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
				}
			}

			try
			{
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, GetEventName(_traceEventPurchase) + " completed: " + _purchaseProductId);
			}
		}

		private void InvokePurchaseFailed(PurchaseResult purchaseResult, StorePurchaseError failReason, Exception ex)
		{
			var product = purchaseResult.TransactionInfo?.Product;
			var productId = _purchaseProductId ?? "null";

			_console.TraceEvent(TraceEventType.Error, _traceEventPurchase, $"{GetEventName(_traceEventPurchase)} error: {productId}, reason = {failReason}");

			if (_observer != null)
			{
				try
				{
					_observer.OnNext(new PurchaseInfo(productId, purchaseResult, failReason, ex));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
				}
			}

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(productId, purchaseResult, failReason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
			}
			finally
			{
				_console.TraceEvent(TraceEventType.Stop, _traceEventPurchase, GetEventName(_traceEventPurchase) + " failed: " + productId);
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			Debug.Assert(product != null);
			Debug.Assert(_storeController != null);

			_console.TraceEvent(TraceEventType.Verbose, _traceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeController.ConfirmPendingPurchase(product);
		}

		private Product InitializeTransaction(string productId)
		{
			Debug.Assert(_purchaseProduct == null);
			Debug.Assert(_storeController != null);

			if (_products.TryGetValue(productId, out _purchaseProduct))
			{
				return _storeController.products.WithID(productId);
			}
			else
			{
				_console.TraceEvent(TraceEventType.Warning, _traceEventPurchase, "No product found for id: " + productId);
			}

			return null;
		}

		private string GetEventName(int eventId)
		{
			switch (eventId)
			{
				case _traceEventInitialize:
					return "Initialize";

				case _traceEventFetch:
					return "Fetch";

				case _traceEventPurchase:
					return "Purchase";
			}

			return "<Unknown>";
		}

		private void ReleaseTransaction()
		{
			_purchaseProductId = null;
			_purchaseProduct = null;
			_purchaseOpCs = null;
		}

		private void ThrowIfDisposed()
		{
			if (_storeListener == null)
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
