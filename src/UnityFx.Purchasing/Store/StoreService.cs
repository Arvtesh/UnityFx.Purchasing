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
		private StoreTransactionProcessor _transaction;

		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<object> _fetchOpCs;
		private IStoreController _storeController;

		#endregion

		#region interface

		internal const int TraceEventInitialize = 1;
		internal const int TraceEventFetch = 2;
		internal const int TraceEventPurchase = 3;

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
			_console = new TraceSource(_serviceName, SourceLevels.All);
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

				if (_transaction != null)
				{
					InvokePurchaseFailed(_transaction.ProductId, new PurchaseResult(_transaction.Product), StorePurchaseError.StoreDisposed, null);
					_transaction.Dispose();
					_transaction = null;
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
		public IStoreServiceSettings Settings => this;

		/// <inheritdoc/>
		public IStoreProductCollection Products => _products;

		/// <inheritdoc/>
		public IStoreController Controller => _storeController;

		/// <inheritdoc/>
		public bool IsInitialized => _storeController != null;

		/// <inheritdoc/>
		public bool IsBusy => _transaction != null;

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
			using (var transaction = new StoreTransactionProcessor(this, productId, false))
			{
				_transaction = transaction;

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
					_transaction = null;
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
		/// Basically just forwards calls to private methods of the parent class.
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
			var transaction = _transaction;

			if (transaction == null)
			{
				var productId = args.purchasedProduct.definition.id;
				transaction = new StoreTransactionProcessor(this, productId, true);
				transaction.Initialize();
			}

			return transaction.ProcessPurchase(args);
		}

		private void OnPurchaseFailed(Product product, PurchaseFailureReason failReason)
		{
			var transaction = _transaction;

			if (transaction == null)
			{
				var productId = product?.definition.id ?? "null";
				transaction = new StoreTransactionProcessor(this, productId, true);
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

		/// <summary>
		/// Represents a store transaction.
		/// </summary>
		/// <remarks>
		/// The class stored all transaction-related data. The transaction begins when the class instance is created
		/// and ends on <see cref="Dispose"/> call.
		/// </remarks>
		internal class StoreTransactionProcessor : IDisposable
		{
			#region data

			private readonly StoreService _storeService;
			private readonly TraceSource _console;
			private readonly string _productId;
			private readonly bool _restored;

			private IStoreProduct _product;
			private TaskCompletionSource<PurchaseResult> _purchaseOpCs;
			private bool _disposed;
			private bool _success;

			#endregion

			#region interface

			public string ProductId => _productId;

			public IStoreProduct Product => _product;

			public StoreTransactionProcessor(StoreService storeService, string productId, bool restored)
			{
				Debug.Assert(storeService != null);
				Debug.Assert(productId != null);

				_storeService = storeService;
				_console = storeService.TraceSource;
				_productId = productId;
				_restored = restored;

				if (restored)
				{
					_console.TraceEvent(TraceEventType.Start, TraceEventPurchase, GetEventName(TraceEventPurchase) + " (auto-restored): " + productId);
				}
				else
				{
					_console.TraceEvent(TraceEventType.Start, TraceEventPurchase, GetEventName(TraceEventPurchase) + ": " + productId);
				}

				storeService.InvokePurchaseInitiated(productId, restored);
			}

			public Product Initialize()
			{
				Debug.Assert(!_disposed);
				Debug.Assert(_storeService.Controller != null);
				Debug.Assert(_storeService.Products != null);

				if (_storeService.Products.TryGetValue(_productId, out _product))
				{
					return _storeService.Controller.products.WithID(_productId);
				}
				else
				{
					_console.TraceEvent(TraceEventType.Warning, TraceEventPurchase, "No product found for id: " + _productId);
				}

				return null;
			}

			public Task<PurchaseResult> Purchase(Product product)
			{
				Debug.Assert(!_disposed);
				Debug.Assert(product != null);
				Debug.Assert(product.definition.id == _productId);
				Debug.Assert(_purchaseOpCs == null);
				Debug.Assert(_storeService.Controller != null);

				_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
				_purchaseOpCs = new TaskCompletionSource<PurchaseResult>(product);
				_storeService.Controller.InitiatePurchase(product);

				return _purchaseOpCs.Task;
			}

			public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
			{
				Debug.Assert(!_disposed);
				Debug.Assert(args != null);
				Debug.Assert(args.purchasedProduct != null);

				var product = args.purchasedProduct;
				var productId = product.definition.id;

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

					// NOTE: _purchaseOp equals to null if this call is a result of purchase restore process,
					// otherwise identifier of the product purchased should match the one specified in _purchaseOp.
					if (_restored || _productId == productId)
					{
						var transactionInfo = new StoreTransaction(product, _restored);

						if (string.IsNullOrEmpty(transactionInfo.Receipt))
						{
							SetPurchaseFailed(transactionInfo, null, StorePurchaseError.ReceiptNullOrEmpty);
						}
						else
						{
							ValidatePurchase(transactionInfo);
							return PurchaseProcessingResult.Pending;
						}
					}
				}
				catch (Exception e)
				{
					SetPurchaseFailed(new StoreTransaction(product, _restored), null, StorePurchaseError.Unknown, e);
				}

				return PurchaseProcessingResult.Complete;
			}

			public void PurchaseFailed(Product product, PurchaseFailureReason failReason)
			{
				Debug.Assert(!_disposed);

				_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, $"OnPurchaseFailed: {_productId}, reason={failReason}");
				SetPurchaseFailed(new StoreTransaction(product, _restored), null, GetPurchaseError(failReason), null);
			}

			#endregion

			#region IDisposable

			public void Dispose()
			{
				if (!_disposed)
				{
					_disposed = true;
					_purchaseOpCs = null;
					_product = null;

					if (_success)
					{
						_console.TraceEvent(TraceEventType.Stop, TraceEventPurchase, GetEventName(TraceEventPurchase) + " completed: " + _productId);
					}
					else
					{
						_console.TraceEvent(TraceEventType.Stop, TraceEventPurchase, GetEventName(TraceEventPurchase) + " failed: " + _productId);
					}
				}
			}

			#endregion

			#region implementation

			private async void ValidatePurchase(StoreTransaction transactionInfo)
			{
				var product = transactionInfo.Product;
				var resultStatus = PurchaseValidationStatus.Failure;

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, $"ValidatePurchase: {product.definition.id}, transactionId = {product.transactionID}");

					var validationResult = await _storeService.ValidatePurchaseAsync(_product, transactionInfo);

					// Do nothing if the store has been disposed while we were waiting for validation.
					if (!_disposed)
					{
						if (validationResult == null)
						{
							// No result returned from the validator means validation succeeded.
							ConfirmPendingPurchase(product);
							SetPurchaseCompleted(transactionInfo, validationResult);
						}
						else
						{
							resultStatus = validationResult.Status;

							if (resultStatus == PurchaseValidationStatus.Ok)
							{
								// The purchase validation succeeded.
								ConfirmPendingPurchase(product);
								SetPurchaseCompleted(transactionInfo, validationResult);
							}
							else if (resultStatus == PurchaseValidationStatus.Failure)
							{
								// The purchase validation failed: confirm to avoid processing it again.
								ConfirmPendingPurchase(product);
								SetPurchaseFailed(transactionInfo, validationResult, StorePurchaseError.ReceiptValidationFailed);
							}
							else
							{
								// Need to re-validate the purchase: do not confirm.
								SetPurchaseFailed(transactionInfo, validationResult, StorePurchaseError.ReceiptValidationNotAvailable);
							}
						}
					}
				}
				catch (Exception e)
				{
					// NOTE: Should not really get here (do we need to confirm it in this case?).
					if (!_disposed)
					{
						ConfirmPendingPurchase(product);
						SetPurchaseFailed(transactionInfo, null, StorePurchaseError.ReceiptValidationFailed, e);
					}
				}
			}

			private void ConfirmPendingPurchase(Product product)
			{
				_console.TraceEvent(TraceEventType.Verbose, TraceEventPurchase, "ConfirmPendingPurchase: " + product.definition.id);
				_storeService.Controller.ConfirmPendingPurchase(product);
			}

			private void SetPurchaseCompleted(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
			{
				var result = new PurchaseResult(_product, transactionInfo, validationResult);

				_success = true;

				if (_purchaseOpCs != null)
				{
					_purchaseOpCs.SetResult(result);
					_purchaseOpCs = null;
				}
				else
				{
					try
					{
						_storeService.InvokePurchaseCompleted(_productId, result);
					}
					finally
					{
						Dispose();
					}
				}
			}

			private void SetPurchaseFailed(StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError failReason, Exception e = null)
			{
				var result = new PurchaseResult(_product, transactionInfo, validationResult);

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

					_purchaseOpCs = null;
				}
				else
				{
					try
					{
						_storeService.InvokePurchaseFailed(_productId, result, failReason, e);
					}
					finally
					{
						Dispose();
					}
				}
			}

			#endregion
		}

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

		private static string GetEventName(int eventId)
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

			try
			{
				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, restored));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		private void InvokePurchaseCompleted(string productId, PurchaseResult purchaseResult)
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
				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(purchaseResult));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
		}

		private void InvokePurchaseFailed(string productId, PurchaseResult purchaseResult, StorePurchaseError failReason, Exception ex)
		{
			_console.TraceEvent(TraceEventType.Error, TraceEventPurchase, $"{GetEventName(TraceEventPurchase)} error: {productId}, reason = {failReason}");

			if (_observer != null)
			{
				try
				{
					_observer.OnNext(new PurchaseInfo(productId, purchaseResult, failReason, ex));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
				}
			}

			try
			{
				PurchaseFailed?.Invoke(this, new PurchaseFailedEventArgs(productId, purchaseResult, failReason, ex));
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, TraceEventPurchase, e);
			}
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
			if (_transaction != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
