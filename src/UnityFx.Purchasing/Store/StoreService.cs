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

		private StoreProductCollection _products;
		private StoreListener _storeListener;
		private StoreObservable _observer;

		private PurchaseOperation _purchaseOperation;

		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<object> _fetchOpCs;
		private IStoreController _storeController;

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
		protected bool IsDisposed => _storeListener != null;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_storeListener = new StoreListener(this);
			_products = new StoreProductCollection();
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
		protected virtual void OnInitialized()
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
			if (disposing && _storeListener != null)
			{
				_storeListener = null;

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

		#region internal interface

		internal enum TraceEventId
		{
			Default,
			Initialize,
			Fetch,
			Purchase
		}

		internal Task<PurchaseValidationResult> ValidatePurchase(IStoreProduct product, StoreTransaction transactionInfo)
		{
			return ValidatePurchaseAsync(product, transactionInfo);
		}

		internal void InvokeInitializeCompleted(int opId)
		{
			try
			{
				OnInitialized();
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

		internal void InvokeInitializeFailed(int opId, StoreInitializeError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, opId, GetEventName(opId) + " error: " + reason);

			try
			{
				OnInitializeFailed(reason, ex);
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

		internal void InvokePurchaseInitiated(string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			try
			{
				OnPurchaseInitiated(productId, restored);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		internal void InvokePurchaseCompleted(string productId, PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			if (_observer != null)
			{
				try
				{
					_observer.OnNext(new PurchaseInfo(productId, purchaseResult, null, null));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
				}
			}

			try
			{
				OnPurchaseCompleted(productId, purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		internal void InvokePurchaseFailed(string productId, PurchaseResult purchaseResult, StorePurchaseError reason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, TraceEventPurchase, $"{GetEventName(TraceEventPurchase)} error: {productId}, reason = {reason}");

			if (_observer != null)
			{
				try
				{
					_observer.OnNext(new PurchaseInfo(productId, purchaseResult, reason, ex));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
				}
			}

			try
			{
				OnPurchaseFailed(productId, purchaseResult, reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		internal static StoreInitializeError GetInitializeError(InitializationFailureReason error)
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

		internal static string GetEventName(int eventId)
		{
			switch (eventId)
			{
				case TraceEventInitialize:
					return "Initialize";

				case TraceEventFetch:
					return "Fetch";

				case TraceEventPurchase:
					return "Purchase";
			}

			return "<Unknown>";
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

						// 4) Trigger user-defined events.
						InvokeInitializeCompleted(TraceEventInitialize);
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

					// 4) Trigger user-defined events.
					InvokeInitializeCompleted(TraceEventFetch);
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
			if (_purchaseOperation != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
