// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	/// <summary>
	/// Unity in-app store service.
	/// </summary>
	/// <example>
	/// The following sample demonstrates usage of this class:
	/// <code>
	/// public class MySimpleStore : StoreService
	/// {
	///     protected override Task&lt;IStoreConfig&gt; GetStoreConfigAsync()
	///     {
	///         var products = new ProductDefinition[] { new ProductDefinition("my_test_product", ProductType.Consumable) };
	///         return Task.FromResult(new StoreConfig(products));
	///     }
	///
	///     protected override ConfigurationBuilder Configure(StoreConfig storeConfig)
	///     {
	///         var purchasingModule = StandardPurchasingModule.Instance();
	///         var configurationBuilder = ConfigurationBuilder.Instance(purchasingModule);
	///         return configurationBuilder.AddProducts(storeConfig.Products);
	///     }
	/// }
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="false"/>
	/// <seealso cref="IStoreService"/>
	public class StoreService : IStoreService
	{
		#region data

		private readonly TraceSource _console;
		private readonly Impl.StoreListener _storeListener;
		private readonly Impl.StoreProductCollection _products;
		private readonly IStoreConfig _defaultStoreConfig;

#if UNITY_IOS
		private readonly AppleValidator _validator;
#else
		private readonly CrossPlatformValidator _validator;
#endif

		private readonly StringBuilder _text;

		private IStoreController _storeController;
		private IExtensionProvider _storeExtensions;

		private Task _initializeTask;
		private Task _fetchTask;

		private int _idCounter;
		private int _busyCounter;
		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// Gets a <see cref="System.Diagnostics.TraceSource"/> instance used by the service.
		/// </summary>
		/// <value>A <see cref="TraceSource"/> instance used for tracing.</value>
		protected TraceSource TraceSource => _console;

		/// <summary>
		/// Gets a <see cref="IStoreListener"/> instance used by the store.
		/// </summary>
		/// <value>A store listener attached to the service.</value>
		protected IStoreListener Listener => _storeListener;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		public StoreService()
			: this(null, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		public StoreService(ProductDefinition[] products)
			: this(products, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		public StoreService(ProductDefinition[] products, byte[] tangleData)
		{
			_console = new TraceSource("Purchasing");
			_text = new StringBuilder(256);
			_storeListener = new Impl.StoreListener(_console, OnRestoredPurchase, OnRestoredPurchaseFailed);
			_products = new Impl.StoreProductCollection();

			if (products != null)
			{
				_defaultStoreConfig = new StoreConfig(products);
			}

			if (tangleData != null)
			{
#if UNITY_IOS
				_validator = new AppleValidator(tangleData);
#else
				_validator = new CrossPlatformValidator(tangleData, tangleData, Application.identifier);
#endif
			}
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect to the app server for information on products available.
		/// </remarks>
		/// <seealso cref="Configure(IStoreConfig)"/>
		/// <seealso cref="ValidatePurchaseAsync(Product)"/>
		protected virtual Task<IStoreConfig> GetStoreConfigAsync()
		{
			return Task.FromResult(_defaultStoreConfig);
		}

		/// <summary>
		/// Configures <c>Unity3d</c> store based on <paramref name="storeConfig"/> data.
		/// </summary>
		/// <remarks>
		/// The method is called when <see cref="GetStoreConfigAsync"/> completes construction of the <c>Unity3d</c> store
		/// configuration. It should use <see cref="IPurchasingModule"/> to create a <see cref="ConfigurationBuilder"/>
		/// instance and then fill it according to <paramref name="storeConfig"/>.
		/// </remarks>
		/// <example>
		/// The code below demostrates possible implementation of the method:
		/// <code>
		/// protected override ConfigurationBuilder Configure(StoreConfig storeConfig)
		/// {
		///     var purchasingModule = StandardPurchasingModule.Instance();
		///     var builder = ConfigurationBuilder.Instance(purchasingModule);
		///     return builder.AddProducts(storeConfig.Products);
		/// }
		/// </code>
		/// </example>
		/// <param name="storeConfig">Store configuration returned by <see cref="GetStoreConfigAsync"/>.</param>
		/// <returns>An instance of <see cref="ConfigurationBuilder"/>.</returns>
		/// <seealso cref="GetStoreConfigAsync"/>
		/// <seealso cref="ValidatePurchaseAsync(Product)"/>
		protected virtual ConfigurationBuilder Configure(IStoreConfig storeConfig)
		{
			var purchasingModule = StandardPurchasingModule.Instance();
			var builder = ConfigurationBuilder.Instance(purchasingModule);

			if (storeConfig != null)
			{
				return builder.AddProducts(storeConfig.Products);
			}

			return builder;
		}

		/// <summary>
		/// Validates a purchase. Inherited classes may override this method if purchase validation is required.
		/// Default implementation does local validation for Android/iOS.
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) initiate server-side validation.
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <seealso cref="GetStoreConfigAsync"/>
		/// <seealso cref="Configure(StoreConfig)"/>
		protected virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(Product product)
		{
			var storeId = string.Empty;
			var nativeReceipt = product.GetNativeReceipt(out storeId);

			if (string.IsNullOrEmpty(nativeReceipt))
			{
				throw new NullReceiptException();
			}
			else if (_validator != null)
			{
				try
				{
#if UNITY_IOS

					var receiptData = Convert.FromBase64String(nativeReceipt);
					var appleReceipt = _validator.Validate(receiptData);
					return Task.FromResult(new PurchaseValidationResult(appleReceipt);

#elif UNITY_ANDROID

					// This code only works for GooglePlay store.
					if (string.CompareOrdinal(storeId, GooglePlay.Name) == 0)
					{
						var result = _validator.Validate(product.receipt);

						if (result == null || result.Length == 0 || result.Length > 1)
						{
							throw new PurchaseValidationException(product);
						}
						else
						{
							return Task.FromResult(new PurchaseValidationResult((GooglePlayReceipt)result[0]);
						}
					}

#endif
				}
				catch (IAPSecurityException e)
				{
					throw new PurchaseValidationException(product, null, e);
				}
			}

			return Task.FromResult(new PurchaseValidationResult(PurchaseValidationStatus.Ok));
		}

		/// <summary>
		/// Called when the store initialize operation has been initiated. Default implementation raises <see cref="InitializeInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnInitializeCompleted(InitializationFailureReason, Exception, int, object)"/>
		protected virtual void OnInitializeInitiated(int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnInitializeInitiated));
				_text.Append(": UnityIAP v");
				_text.Append(StandardPurchasingModule.k_PackageVersion);
				_text.Append('.');

				_console.TraceEvent(TraceEventType.Start, opId, _text.ToString());

				InitializeInitiated?.Invoke(this, new FetchInitiatedEventArgs(opId, userState));
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store initialization has succeeded. Default implementation raises <see cref="InitializeCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnInitializeInitiated(int, object)"/>
		protected virtual void OnInitializeCompleted(InitializationFailureReason? failReason, Exception e, int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnInitializeCompleted));

				if (failReason != null || e != null)
				{
					_text.Append(", failReason: ");

					if (failReason != null)
					{
						_text.Append(failReason.ToString());
					}
					else
					{
						_text.Append(e.GetType().ToString());
					}

					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, opId, _text.ToString());

				InitializeCompleted?.Invoke(this, new FetchCompletedEventArgs(failReason, e, opId, userState));
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated. Default implementation raises <see cref="FetchInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnFetchCompleted(FetchError, Exception, int, object)"/>
		protected virtual void OnFetchInitiated(int opId, object userState)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Start, opId, nameof(OnFetchInitiated));

				FetchInitiated?.Invoke(this, new FetchInitiatedEventArgs(opId, userState));
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store fetch has succeeded. Default implementation raises <see cref="FetchCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnFetchInitiated(int, object)"/>
		protected virtual void OnFetchCompleted(InitializationFailureReason? failReason, Exception e, int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnFetchCompleted));

				if (failReason != null)
				{
					_text.Append(": ");
					_text.Append(failReason.ToString());
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, opId, _text.ToString());

				FetchCompleted?.Invoke(this, new FetchCompletedEventArgs(failReason, e, opId, userState));
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the restore operation has been initiated.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnRestoreCompleted(Exception, int, object)"/>
		protected virtual void OnRestoreInitiated(int opId, object userState)
		{
			try
			{
				_console.TraceEvent(TraceEventType.Start, opId, nameof(OnRestoreInitiated));

				// TODO
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store fetch has succeeded. Default implementation raises <see cref="FetchCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnRestoreInitiated(int, object)"/>
		protected virtual void OnRestoreCompleted(Exception e, int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnRestoreCompleted));

				if (e != null)
				{
					_text.Append(": ");
					_text.Append(e.GetType().Name);
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, opId, _text.ToString());

				// TODO
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated. Default implementation raises <see cref="PurchaseInitiated"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnPurchaseCompleted(string, PurchaseFailureReason?, PurchaseValidationResult, Exception, bool, int, object)"/>
		protected virtual void OnPurchaseInitiated(string productId, bool restored, int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnPurchaseInitiated));
				_text.Append(": ");
				_text.Append(productId);

				if (restored)
				{
					_text.Append(", auto-restored.");
				}
				else
				{
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Start, opId, _text.ToString());

				PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(productId, restored, opId, userState));
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Called when the store purchase operation succeded. Default implementation raises <see cref="PurchaseCompleted"/> event.
		/// </summary>
		/// <remarks>
		/// The implementation should not throw exceptions.
		/// </remarks>
		/// <seealso cref="OnPurchaseInitiated(string, bool, int, object)"/>
		protected virtual void OnPurchaseCompleted(string productId, Product product, PurchaseValidationResult validationResult, PurchaseFailureReason? failReason, Exception e, bool restored, int opId, object userState)
		{
			try
			{
				_text.Clear();
				_text.Append(nameof(OnPurchaseCompleted));
				_text.Append(": ");
				_text.Append(productId);

				if (failReason != null || e != null)
				{
					_text.Append(", failReason: ");

					if (failReason != null)
					{
						_text.Append(failReason.ToString());
					}
					else
					{
						_text.Append(e.GetType().ToString());
					}
				}

				_text.Append(", validationResult: ");
				_text.Append(validationResult.Status.ToString());

				if (restored)
				{
					_text.Append(", auto-restored.");
				}
				else
				{
					_text.Append('.');
				}

				_console.TraceEvent(TraceEventType.Stop, opId, _text.ToString());

				PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(productId, product, validationResult, failReason, e, restored, opId, userState));
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
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
				_storeController = null;
				_storeExtensions = null;

				_products.SetController(null);
				_storeListener.Dispose();

				_console.TraceEvent(TraceEventType.Verbose, 0, "Disposed");
			}
		}

		/// <summary>
		/// Throws an <see cref="ObjectDisposedException"/> if the instance is already disposed.
		/// </summary>
		/// <seealso cref="Dispose()"/>
		/// <seealso cref="Dispose(bool)"/>
		/// <seealso cref="ThrowIfPlatformNotSupported"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		/// <seealso cref="ThrowIfBusy"/>
		protected void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		/// <summary>
		/// Throws an <see cref="PlatformNotSupportedException"/> if the platform is not supported by the store.
		/// </summary>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		/// <seealso cref="ThrowIfBusy"/>
		protected void ThrowIfPlatformNotSupported()
		{
			// TODO
		}

		/// <summary>
		/// Throws an <see cref="ArgumentException"/> if the specified <paramref name="productId"/> is <see langword="null"/> or empty string.
		/// </summary>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfPlatformNotSupported"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		/// <seealso cref="ThrowIfBusy"/>
		protected void ThrowIfInvalidProductId(string productId)
		{
			if (productId == null)
			{
				throw new ArgumentNullException("In-app product identifier cannot be null.", nameof(productId));
			}

			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException("In-app product identifier cannot be an empty string.", nameof(productId));
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the service is not initialized yet.
		/// </summary>
		/// <seealso cref="IsInitialized"/>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfPlatformNotSupported"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfBusy"/>
		protected void ThrowIfNotInitialized()
		{
			if (_storeController == null)
			{
				throw new InvalidOperationException("Purchasing is not initialized.");
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if a purchase operation is currently running.
		/// </summary>
		/// <seealso cref="IsBusy"/>
		/// <seealso cref="ThrowIfDisposed"/>
		/// <seealso cref="ThrowIfPlatformNotSupported"/>
		/// <seealso cref="ThrowIfInvalidProductId(string)"/>
		/// <seealso cref="ThrowIfNotInitialized"/>
		protected void ThrowIfBusy()
		{
			if (_busyCounter > 0)
			{
				throw new InvalidOperationException("Purchasing service is busy.");
			}
		}

		#endregion

		#region IStoreService

		/// <inheritdoc/>
		public event EventHandler<FetchInitiatedEventArgs> InitializeInitiated;

		/// <inheritdoc/>
		public event EventHandler<FetchCompletedEventArgs> InitializeCompleted;

		/// <inheritdoc/>
		public event EventHandler<FetchInitiatedEventArgs> FetchInitiated;

		/// <inheritdoc/>
		public event EventHandler<FetchCompletedEventArgs> FetchCompleted;

		/// <inheritdoc/>
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <inheritdoc/>
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		/// <inheritdoc/>
		public TraceListenerCollection TraceListeners
		{
			get
			{
				return _console.Listeners;
			}
		}

		public SourceSwitch TraceSwitch
		{
			get
			{
				return _console.Switch;
			}
		}

		/// <inheritdoc/>
		public IStoreProductCollection<Product> Products
		{
			get
			{
				return _products;
			}
		}

		/// <inheritdoc/>
		public IStoreController Controller
		{
			get
			{
				return _storeController;
			}
		}

		/// <inheritdoc/>
		public IExtensionProvider Extensions
		{
			get
			{
				return _storeExtensions;
			}
		}

		/// <inheritdoc/>
		public bool IsInitialized
		{
			get
			{
				return _storeController != null;
			}
		}

		/// <inheritdoc/>
		public bool IsBusy
		{
			get
			{
				return _busyCounter > 0;
			}
		}

		/// <inheritdoc/>
		public Task InitializeAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			return InitializeInternal(null);
		}

		/// <inheritdoc/>
		public Task FetchAsync()
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			return FetchInternal(null);
		}

		/// <inheritdoc/>
		public Task RestoreAsync()
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			return RestoreInternal(null);
		}

		/// <inheritdoc/>
		public Task<PurchaseResult> PurchaseAsync(string productId, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfInvalidProductId(productId);
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			return PurchaseInternal(productId, stateObject);
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

		private Task InitializeInternal(object asyncState)
		{
			if (_storeController == null)
			{
				if (_initializeTask == null)
				{
					var task = InitializeUnsafe(asyncState);

					if (task.IsCompleted)
					{
						return task;
					}

					_initializeTask = task;
				}

				return _initializeTask;
			}

			return Task.CompletedTask;
		}

		private async Task InitializeUnsafe(object asyncState)
		{
			if (_storeController == null)
			{
				var id = ++_idCounter;

				_console.TraceEvent(TraceEventType.Information, id, nameof(InitializeAsync));

				try
				{
					SetBusy(true);
					OnInitializeInitiated(id, asyncState);

					// 1) Load the store configuration.
					var config = await GetStoreConfigAsync();
					var builder = Configure(config);

					// 2) Initialize in-app store.
					await _storeListener.InitializeAsync(builder, id, asyncState);

					// 3) Force a delay between the init operation completion and its continuation code.
					// Otherwise UnityIAP might behave weird (for ex. calling ProcessPurchase twice).
					await Task.Yield();

					// 4) Finalize.
					_storeController = _storeListener.Controller;
					_storeExtensions = _storeListener.Extensions;
					_products.SetController(_storeController);

					OnInitializeCompleted(null, null, id, asyncState);
				}
				catch (FetchException e)
				{
					OnInitializeCompleted(e.ErrorCode, e, id, asyncState);
					throw;
				}
				catch (Exception e)
				{
					OnInitializeCompleted(null, e, id, asyncState);
					throw;
				}
				finally
				{
					_initializeTask = null;
					SetBusy(false);
				}
			}
		}

		private Task FetchInternal(object asyncState)
		{
			if (_fetchTask == null)
			{
				var task = FetchUnsafe(asyncState);

				if (task.IsCompleted)
				{
					return task;
				}

				_fetchTask = task;
			}

			return _fetchTask;
		}

		private async Task FetchUnsafe(object asyncState)
		{
			Debug.Assert(_storeController != null);

			var id = ++_idCounter;

			_console.TraceEvent(TraceEventType.Information, id, nameof(FetchAsync));

			try
			{
				SetBusy(true);
				OnFetchInitiated(id, asyncState);

				// 1) Load the store configuration.
				var config = await GetStoreConfigAsync();

				// 2) Fetch in-app store.
				await _storeListener.FetchAsync(config, id, asyncState);

				// 3) Force a delay between the init operation completion and its continuation code.
				await Task.Yield();

				// 4) Finalize.
				OnFetchCompleted(null, null, id, asyncState);
			}
			catch (FetchException e)
			{
				OnFetchCompleted(e.ErrorCode, e, id, asyncState);
				throw;
			}
			catch (Exception e)
			{
				OnFetchCompleted(null, e, id, asyncState);
				throw;
			}
			finally
			{
				_fetchTask = null;
				SetBusy(false);
			}
		}

		private async Task RestoreInternal(object asyncState)
		{
			var id = ++_idCounter;

			_console.TraceEvent(TraceEventType.Information, id, $"{nameof(RestoreAsync)}");

			try
			{
				SetBusy(true);
				OnRestoreInitiated(id, asyncState);

				// 1) Restore transactions.
				await _storeListener.RestoreAsync(id, asyncState);

				// 2) Force a delay between the operation completion and its continuation code.
				await Task.Yield();

				// 4) Finalize.
				OnRestoreCompleted(null, id, asyncState);
			}
			catch (Exception e)
			{
				OnRestoreCompleted(e, id, asyncState);
				throw;
			}
			finally
			{
				SetBusy(false);
			}
		}

		private async Task<PurchaseResult> PurchaseInternal(string productId, object asyncState)
		{
			Debug.Assert(productId != null);

			var id = ++_idCounter;

			_console.TraceEvent(TraceEventType.Information, id, $"{nameof(PurchaseAsync)}: {productId}.");

			try
			{
				SetBusy(true);

				// 1) Initialize the store if it is not initialized yet.
				await InitializeInternal(asyncState);

				// 2) Await pending fetch operation (if any).
				if (_fetchTask != null)
				{
					await _fetchTask;
				}

				// 3) Make sure the product exists in the store.
				if (_storeController.products.WithID(productId) == null)
				{
					throw new InvalidOperationException($"Product {productId} does not exist in the store.");
				}

				try
				{
					OnPurchaseInitiated(productId, false, id, asyncState);

					// 4) Purchase the product.
					var product = await _storeListener.PurchaseAsync(productId, id, asyncState);

					// 5) Validate the purchased product receipt.
					var validationResult = await ValidateInternal(product);

					// 6) Confirm the transaction if validation completed with status PurchaseValidationStatus.Ok or PurchaseValidationStatus.Failed.
					if (validationResult.Status != PurchaseValidationStatus.NotAvailable)
					{
						_console.TraceEvent(TraceEventType.Information, id, $"{nameof(IStoreController.ConfirmPendingPurchase)}: {productId}.");
						_storeController.ConfirmPendingPurchase(product);
					}

					// 7) Finalize.
					OnPurchaseCompleted(productId, product, validationResult, null, null, false, id, asyncState);
					return new PurchaseResult(product, validationResult, false);
				}
				catch (PurchaseValidationException e)
				{
					// Should confirm the transaction when validation fails to avoid processing the invalid product again (as restored purchase).
					_console.TraceEvent(TraceEventType.Information, id, $"{nameof(IStoreController.ConfirmPendingPurchase)}: {productId}.");
					_storeController.ConfirmPendingPurchase(e.Product);

					OnPurchaseCompleted(productId, e.Product, new PurchaseValidationResult(PurchaseValidationStatus.Failed), null, e, false, id, asyncState);
					throw;
				}
				catch (PurchaseException e)
				{
					OnPurchaseCompleted(productId, e.Product, new PurchaseValidationResult(), e.ErrorCode, e, false, id, asyncState);
					throw;
				}
				catch (Exception e)
				{
					OnPurchaseCompleted(productId, null, new PurchaseValidationResult(), PurchaseFailureReason.Unknown, e, false, id, asyncState);
					throw;
				}
			}
			finally
			{
				SetBusy(false);
			}
		}

		private async void OnRestoredPurchase(Product product)
		{
			Debug.Assert(product != null);

			var productId = product.definition.id;

			// NOTE: This method should not throw exceptions.
			try
			{
				SetBusy(true);
				OnPurchaseInitiated(productId, true, 0, null);

				// 1) Validate the purchased product receipt.
				var validationResult = await ValidateInternal(product);

				// 2) Confirm the transaction if validation completed with status PurchaseValidationStatus.Ok or PurchaseValidationStatus.Failed.
				if (validationResult.Status != PurchaseValidationStatus.NotAvailable)
				{
					_console.TraceEvent(TraceEventType.Information, 0, $"{nameof(IStoreController.ConfirmPendingPurchase)}: {productId}.");
					_storeController.ConfirmPendingPurchase(product);
				}

				// 3) Finalize.
				OnPurchaseCompleted(productId, product, validationResult, null, null, true, 0, null);
			}
			catch (PurchaseValidationException e)
			{
				_console.TraceEvent(TraceEventType.Information, 0, $"{nameof(IStoreController.ConfirmPendingPurchase)}: {productId}.");
				_storeController.ConfirmPendingPurchase(e.Product);

				OnPurchaseCompleted(productId, e.Product, new PurchaseValidationResult(PurchaseValidationStatus.Failed), null, e, false, 0, null);
			}
			catch (Exception e)
			{
				OnPurchaseCompleted(productId, null, new PurchaseValidationResult(), PurchaseFailureReason.Unknown, e, false, 0, null);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private void OnRestoredPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			Debug.Assert(product != null);

			// NOTE: This method should not throw exceptions.
			try
			{
				SetBusy(true);
				OnPurchaseInitiated(product.definition.id, true, 0, null);
				OnPurchaseCompleted(product.definition.id, product, new PurchaseValidationResult(), reason, new PurchaseException(product, reason), true, 0, null);
			}
			finally
			{
				SetBusy(false);
			}
		}

		private async Task<PurchaseValidationResult> ValidateInternal(Product product)
		{
			Debug.Assert(product != null);

			try
			{
				return await ValidatePurchaseAsync(product);
			}
			catch (PurchaseValidationException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new PurchaseValidationException(product, null, e);
			}
		}

		private void SetBusy(bool busy)
		{
			if (busy)
			{
				++_busyCounter;
			}
			else
			{
				--_busyCounter;
			}
		}

		#endregion
	}
}
