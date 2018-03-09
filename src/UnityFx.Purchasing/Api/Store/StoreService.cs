// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	internal enum StoreOperationType
	{
		Unknown,
		Initialize,
		Fetch,
		Purchase
	}

	/// <summary>
	/// An in-app store based on <c>Unity IAP</c>.
	/// </summary>
	/// <example>
	/// The following sample demonstrates usage of this class:
	/// <code>
	/// public class MySimpleStore : StoreService
	/// {
	///     protected override IAsyncOperation&lt;StoreConfig&gt; GetStoreConfig()
	///     {
	///         var products = new ProductDefinition[] { new ProductDefinition("my_test_product", ProductType.Consumable) };
	///         return AsyncResult.FromResult(new StoreConfig(products));
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
	public abstract class StoreService : IStoreService, IDisposable
	{
		#region data

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly StoreListener _storeListener;
		private readonly SynchronizationContext _syncContext;
		private readonly StoreServiceSettings _settings;

		private StoreProductCollection _products;
#if !NET35
		private StoreObservable<PurchaseResult> _purchases;
		private StoreObservable<FailedPurchaseResult> _failedPurchases;
#endif
		private IStoreController _storeController;
		private IExtensionProvider _storeExtensions;
		private bool _disposed;

		#endregion

		#region interface

		/// <summary>
		/// Gets the store settings.
		/// </summary>
		/// <value>An instance of store settings controller.</value>
		public IStoreServiceSettings Settings => _settings;

		/// <summary>
		/// Gets a <see cref="System.Diagnostics.TraceSource"/> instance used by the service.
		/// </summary>
		/// <value>A <see cref="TraceSource"/> instance used for tracing.</value>
		protected internal TraceSource TraceSource => _console;

		/// <summary>
		/// Gets a <see cref="SynchronizationContext"/> instance used to forward execution to the main thread (can be <see langword="null"/>).
		/// </summary>
		/// <value>An object used to forward execution to the main thread.</value>
		protected internal SynchronizationContext SyncContext => _syncContext;

		/// <summary>
		/// Gets a <see cref="IStoreListener"/> instance used by the store.
		/// </summary>
		/// <value>A store listener attached to the service.</value>
		protected internal IStoreListener Listener => _storeListener;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService()
			: this(null, SynchronizationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(SynchronizationContext syncContext)
			: this(null, syncContext)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		protected StoreService(string name)
			: this(name, SynchronizationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		/// <param name="name">Name of the store service (<c>Purchasing</c> is used by default).</param>
		/// <param name="syncContext">A synchronization context used to forward execution to the main thread.</param>
		protected StoreService(string name, SynchronizationContext syncContext)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : name;
			_console = new TraceSource(_serviceName);
			_storeListener = new StoreListener(this);
			_syncContext = syncContext;
			_settings = new StoreServiceSettings(_console, _storeListener);
		}

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect to the app server for information on products available.
		/// </remarks>
		/// <seealso cref="Configure(StoreConfig)"/>
		/// <seealso cref="ValidatePurchase(IStoreTransaction)"/>
		protected internal abstract IAsyncOperation<StoreConfig> GetStoreConfig();

		/// <summary>
		/// Configures <c>Unity3d</c> store based on <paramref name="storeConfig"/> data.
		/// </summary>
		/// <remarks>
		/// The method is called when <see cref="GetStoreConfig"/> completes to construct <c>Unity3d</c> store
		/// configuration. It should use <see cref="IPurchasingModule"/> to create a <see cref="ConfigurationBuilder"/>
		/// instance and then fill it according to <paramref name="storeConfig"/>.
		/// </remarks>
		/// <example>
		/// The code below demostrates typical implementation of this method:
		/// <code>
		/// protected override ConfigurationBuilder Configure(StoreConfig storeConfig)
		/// {
		///     var purchasingModule = StandardPurchasingModule.Instance();
		///     var configurationBuilder = ConfigurationBuilder.Instance(purchasingModule);
		///     return configurationBuilder.AddProducts(storeConfig.Products);
		/// }
		/// </code>
		/// </example>
		/// <param name="storeConfig">Store configuration returned by <see cref="GetStoreConfig"/>.</param>
		/// <returns>An instance of <see cref="ConfigurationBuilder"/>.</returns>
		/// <seealso cref="GetStoreConfig"/>
		/// <seealso cref="ValidatePurchase(IStoreTransaction)"/>
		protected internal abstract ConfigurationBuilder Configure(StoreConfig storeConfig);

		/// <summary>
		/// Validates a purchase. Inherited classes may override this method if purchase validation is required.
		/// Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) initiate server-side validation.
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <seealso cref="GetStoreConfig"/>
		/// <seealso cref="Configure(StoreConfig)"/>
		protected internal virtual IAsyncOperation<PurchaseValidationResult> ValidatePurchase(IStoreTransaction transactionInfo)
		{
			return null;
		}

		/// <summary>
		/// Called when the store initialize operation has been initiated. Default implementation raises <see cref="InitializeInitiated"/> event.
		/// </summary>
		/// <seealso cref="OnInitializeCompleted(IStoreOperationInfo, StoreFetchError, Exception)"/>
		protected internal virtual void OnInitializeInitiated(IStoreOperationInfo op)
		{
			InitializeInitiated?.Invoke(this, new FetchInitiatedEventArgs(op));
		}

		/// <summary>
		/// Called when the store initialization has succeeded. Default implementation raises <see cref="InitializeCompleted"/> event.
		/// </summary>
		/// <seealso cref="OnInitializeInitiated(IStoreOperationInfo)"/>
		protected internal virtual void OnInitializeCompleted(IStoreOperationInfo op, StoreFetchError failReason, Exception e)
		{
			InitializeCompleted?.Invoke(this, new FetchCompletedEventArgs(op, failReason, e));
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated. Default implementation raises <see cref="FetchInitiated"/> event.
		/// </summary>
		/// <seealso cref="OnFetchCompleted(IStoreOperationInfo, StoreFetchError, Exception)"/>
		protected internal virtual void OnFetchInitiated(IStoreOperationInfo op)
		{
			FetchInitiated?.Invoke(this, new FetchInitiatedEventArgs(op));
		}

		/// <summary>
		/// Called when the store fetch has succeeded. Default implementation raises <see cref="FetchCompleted"/> event.
		/// </summary>
		/// <seealso cref="OnFetchInitiated(IStoreOperationInfo)"/>
		protected internal virtual void OnFetchCompleted(IStoreOperationInfo op, StoreFetchError failReason, Exception e)
		{
			FetchCompleted?.Invoke(this, new FetchCompletedEventArgs(op, failReason, e));
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated. Default implementation raises <see cref="PurchaseInitiated"/> event.
		/// </summary>
		/// <seealso cref="OnPurchaseCompleted(IPurchaseResult, StorePurchaseError, Exception)"/>
		protected internal virtual void OnPurchaseInitiated(IStoreOperationInfo op, string productId, bool restored)
		{
			PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(op, productId, restored));
		}

		/// <summary>
		/// Called when the store purchase operation succeded. Default implementation raises <see cref="PurchaseCompleted"/> event.
		/// </summary>
		/// <seealso cref="OnPurchaseInitiated(IStoreOperationInfo, string, bool)"/>
		protected internal virtual void OnPurchaseCompleted(IPurchaseResult result, StorePurchaseError failReason, Exception e)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(result, failReason, e));

#if UNITYFX_SUPPORT_OBSERVABLES

			if (failReason == StorePurchaseError.None)
			{
				_purchases?.OnNext(new PurchaseResult(result));
			}
			else
			{
				_failedPurchases?.OnNext(new FailedPurchaseResult(result, failReason, e));
			}

#endif
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

				SetStoreController(null, null);

#if UNITYFX_SUPPORT_OBSERVABLES

				_purchases?.OnCompleted();
				_failedPurchases?.OnCompleted();

#endif

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
				throw new ObjectDisposedException(_serviceName);
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
				throw new ArgumentNullException(_serviceName + " product identifier cannot be null.", nameof(productId));
			}

			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException(_serviceName + " product identifier cannot be an empty string.", nameof(productId));
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
				throw new InvalidOperationException(_serviceName + " is not initialized.");
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
			if (_storeListener.IsBusy)
			{
				throw new InvalidOperationException(_serviceName + " is busy.");
			}
		}

		#endregion

		#region internals

		internal void SetStoreController(IStoreController controller, IExtensionProvider extensions)
		{
			_storeController = controller;
			_storeExtensions = extensions;
			_products?.SetController(controller);
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

#if UNITYFX_SUPPORT_OBSERVABLES

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

		/// <inheritdoc/>
		public IStoreProductCollection<Product> Products
		{
			get
			{
				if (_products == null)
				{
					_products = new StoreProductCollection(_storeController);
				}

				return _products;
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IStoreController Controller
		{
			get
			{
				return _storeController;
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
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
				return _storeListener.IsBusy;
			}
		}

		/// <inheritdoc/>
		public IAsyncOperation InitializeAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			if (_storeController == null)
			{
				return _storeListener.InitializeOp ?? InitializeInternal(null, null);
			}

			return AsyncResult.CompletedOperation;
		}

		/// <inheritdoc/>
		public IAsyncOperation FetchAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfNotInitialized();

			return _storeListener.FetchOp ?? FetchInternal(null, null);
		}

		/// <inheritdoc/>
		public IAsyncOperation<PurchaseResult> PurchaseAsync(string productId, object stateObject = null)
		{
			ThrowIfDisposed();
			ThrowIfInvalidProductId(productId);
			ThrowIfPlatformNotSupported();

			return PurchaseInternal(productId, null, stateObject);
		}

#if UNITYFX_SUPPORT_APM

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInitialize(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfInitialized();

			return _storeListener.InitializeOp ?? InitializeInternal(userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void EndInitialize(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation<object>(asyncResult, StoreOperationType.Initialize))
			{
				op.Join();
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginFetch(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfNotInitialized();

			return _storeListener.FetchOp ?? FetchInternal(userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void EndFetch(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation<object>(asyncResult, StoreOperationType.Fetch))
			{
				op.Join();
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginPurchase(string productId, AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfInvalidProductId(productId);
			ThrowIfPlatformNotSupported();

			return PurchaseInternal(productId, userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public PurchaseResult EndPurchase(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation<PurchaseResult>(asyncResult, StoreOperationType.Purchase))
			{
				return op.Join();
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		/// <inheritdoc/>
		public Task InitializeTaskAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			if (_storeController == null)
			{
				if (_storeListener.InitializeOp != null)
				{
					return _storeListener.InitializeOp.ToTask();
				}
				else
				{
					var tcs = new TaskCompletionSource<object>();
					InitializeInternal(FetchCompletionCallback, tcs);
					return tcs.Task;
				}
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		public Task FetchTaskAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfNotInitialized();

			if (_storeListener.FetchOp != null)
			{
				return _storeListener.FetchOp.ToTask();
			}
			else
			{
				var tcs = new TaskCompletionSource<object>();
				FetchInternal(FetchCompletionCallback, tcs);
				return tcs.Task;
			}
		}

		/// <inheritdoc/>
		public Task<PurchaseResult> PurchaseTaskAsync(string productId, object stateObject = null)
		{
			ThrowIfDisposed();
			ThrowIfInvalidProductId(productId);
			ThrowIfPlatformNotSupported();

			var tcs = new TaskCompletionSource<PurchaseResult>(stateObject);

			PurchaseInternal(
				productId,
				op =>
				{
					var storeOp = op as IAsyncOperation<PurchaseResult>;

					if (storeOp.IsCompletedSuccessfully)
					{
						tcs.TrySetResult(storeOp.Result);
					}
					else if (storeOp.IsCanceled)
					{
						tcs.TrySetCanceled();
					}
					else
					{
						tcs.TrySetException(storeOp.Exception.InnerExceptions);
					}
				},
				stateObject);

			return tcs.Task;
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

		private InitializeOperation InitializeInternal(AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert(_storeListener.InitializeOp == null);
			Debug.Assert(_storeController == null);

			var result = _storeListener.BeginInitialize(userCallback, stateObject);

			try
			{
				result.Start();
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

		private FetchOperation FetchInternal(AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert(_storeListener.FetchOp == null);
			Debug.Assert(_storeController != null);

			var result = _storeListener.BeginFetch(userCallback, stateObject);

			try
			{
				result.Start();
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

		private PurchaseOperation PurchaseInternal(string productId, AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			var result = _storeListener.BeginPurchase(productId, false, userCallback, stateObject);

			try
			{
				StoreOperation<object> fetchOp = null;

				if (_storeController == null)
				{
					fetchOp = _storeListener.InitializeOp ?? InitializeInternal(null, null);
				}
				else
				{
					fetchOp = _storeListener.FetchOp;
				}

				if (fetchOp != null)
				{
					result.SetScheduled();

					fetchOp.AddCompletionCallback(
						op =>
						{
							if (!_disposed)
							{
								if (fetchOp.IsCompletedSuccessfully)
								{
									try
									{
										_storeListener.Enqueue(result);
									}
									catch (Exception e)
									{
										result.SetFailed(e);
									}
								}
								else
								{
									result.SetFailed(fetchOp.Exception);
								}
							}
						},
						false);
				}
				else
				{
					_storeListener.Enqueue(result);
				}
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

#if UNITYFX_SUPPORT_APM

		private StoreOperation<T> ValidateOperation<T>(IAsyncResult asyncResult, StoreOperationType type)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException(nameof(asyncResult));
			}

			if (asyncResult is StoreOperation<T> result)
			{
				result.Validate(_storeListener, type);
				return result;
			}
			else
			{
				throw new ArgumentException("Invalid operation type", nameof(asyncResult));
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		private static void FetchCompletionCallback(IAsyncResult asyncResult)
		{
			var storeOp = asyncResult as IAsyncOperation;
			var tcs = asyncResult.AsyncState as TaskCompletionSource<object>;

			if (storeOp.IsCompletedSuccessfully)
			{
				tcs.TrySetResult(null);
			}
			else if (storeOp.IsCanceled)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetException(storeOp.Exception.InnerExceptions);
			}
		}

#endif

		private void ThrowIfInitialized()
		{
			if (_storeController != null)
			{
				throw new InvalidOperationException(_serviceName + " is already initialized.");
			}
		}

		#endregion
	}
}
