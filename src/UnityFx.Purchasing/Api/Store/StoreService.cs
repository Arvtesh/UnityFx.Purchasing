// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

	[Flags]
	internal enum StoreOperationType
	{
		Initialize = 1,
		InitializeEap = Initialize | 4,
		InitializeApm = Initialize | 8,
		InitializeTap = Initialize | 6,

		Fetch = 2,
		FetchEap = Fetch | 4,
		FetchApm = Fetch | 8,
		FetchTap = Fetch | 6,

		Purchase = 3,
		PurchaseEap = Purchase | 4,
		PurchaseApm = Purchase | 8,
		PurchaseTap = Purchase | 6,
	}

	/// <summary>
	/// Implementation of <see cref="IStoreService"/>.
	/// </summary>
	/// <example>
	/// The following sample demonstrates usage of this class:
	/// <code>
	/// public class MySimpleStore : StoreService
	/// {
	///     public MySimpleStore()
	///         : base(null, StandardPurchasingModule.Instance())
	///     {
	///     }
	///
	///     protected override void GetStoreConfig(Action&lt;StoreConfig&gt; onSuccess, Action&lt;Exception&gt; onFailure)
	///     {
	///         var products = new ProductDefinition[] { new ProductDefinition("test_product", ProductType.Consumable) };
	///         onSuccess(new StoreConfig(products));
	///     }
	/// }
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="false"/>
	/// <seealso cref="IStoreService"/>
	public abstract class StoreService : IStoreService, IStoreServiceSettings
	{
		#region data

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly StoreListener _storeListener;
		private readonly IPurchasingModule _purchasingModule;
		private readonly SynchronizationContext _syncContext;

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
		/// Returns the <see cref="System.Diagnostics.TraceSource"/> instance used by the service. Read only.
		/// </summary>
		protected internal TraceSource TraceSource => _console;

		/// <summary>
		/// Returns <see cref="SynchronizationContext"/> instance used to forward execution to the main thread. Read only.
		/// </summary>
		protected internal SynchronizationContext SyncContext => _syncContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(IPurchasingModule purchasingModule)
			: this(null, purchasingModule, SynchronizationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(IPurchasingModule purchasingModule, SynchronizationContext syncContext)
			: this(null, purchasingModule, syncContext)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
			: this(name, purchasingModule, SynchronizationContext.Current)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule, SynchronizationContext syncContext)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_storeListener = new StoreListener(this);
			_syncContext = syncContext;
		}

		/// <summary>
		/// Executes the delegate passed on sync context (if present). Otherwise just invokes the delegate.
		/// </summary>
		protected internal void QueueOnMainThread(SendOrPostCallback action, object args)
		{
			Debug.Assert(action != null);

			if (!_disposed)
			{
				if (_syncContext != null)
				{
					_syncContext.Post(action, args);
				}
				else
				{
					action(args);
				}
			}
		}

#if UNITYFX_SUPPORT_TAP

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect to the app server for information on products available.
		/// </remarks>
		/// <seealso cref="ValidatePurchaseAsync(IStoreTransaction)"/>
		protected internal abstract Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates a purchase. Inherited classes may override this method if purchase validation is required.
		/// </summary>
		/// <remarks>
		/// <para>Typical implementation would first do client validation of the purchase and (if that passes)
		/// initiate server-side validation.</para>
		/// <para>Throwing an exception from this method or returning a faulted/canceled task results in
		/// failed transaction validation.</para>
		/// </remarks>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <seealso cref="GetStoreConfigAsync()"/>
		protected internal virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(IStoreTransaction transactionInfo)
		{
			return Task.FromResult<PurchaseValidationResult>(null);
		}

#else

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect to the app server for information on products available.
		/// </remarks>
		/// <param name="completionSource">A provider of completion notification.</param>
		/// <seealso cref="ValidatePurchase(IStoreTransaction, IAsyncCompletionSource{PurchaseValidationResult})"/>
		protected internal abstract void GetStoreConfig(IAsyncCompletionSource<StoreConfig> completionSource);

		/// <summary>
		/// Validates a purchase. Inherited classes may override this method if purchase validation is required.
		/// Default implementation just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) initiate server-side validation.
		/// </remarks>
		/// <returns>Returns <see langword="true"/> if validation is implemented; <see langword="false"/> if not.</returns>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		/// <param name="completionSource">A provider of completion notification.</param>
		/// <seealso cref="GetStoreConfig(IAsyncCompletionSource{StoreConfig})"/>
		protected internal virtual bool ValidatePurchase(IStoreTransaction transactionInfo, IAsyncCompletionSource<PurchaseValidationResult> completionSource)
		{
			return false;
		}

#endif

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
			if (!IsPlatformSupported())
			{
				throw new PlatformNotSupportedException();
			}
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
			if (_storeListener.PurchaseOp != null)
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
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IStoreController Controller
		{
			get
			{
				ThrowIfDisposed();
				ThrowIfNotInitialized();
				return _storeController;
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IExtensionProvider Extensions
		{
			get
			{
				ThrowIfDisposed();
				ThrowIfNotInitialized();
				return _storeExtensions;
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
				return _storeListener.PurchaseOp != null;
			}
		}

		/// <inheritdoc/>
		public IStoreOperation InitializeAsync()
		{
			return InitializeAsync(null);
		}

		/// <inheritdoc/>
		public IStoreOperation InitializeAsync(object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfInitialized();
			ThrowIfInitializePending();

			return InitializeInternal(StoreOperationType.InitializeEap, null, stateObject);
		}

		/// <inheritdoc/>
		public IStoreOperation FetchAsync()
		{
			return FetchAsync(null);
		}

		/// <inheritdoc/>
		public IStoreOperation FetchAsync(object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfNotInitialized();
			ThrowIfFetchPending();

			return FetchInternal(StoreOperationType.FetchEap, null, stateObject);
		}

		/// <inheritdoc/>
		public IStoreOperation<PurchaseResult> PurchaseAsync(string productId)
		{
			return PurchaseAsync(productId, null);
		}

		/// <inheritdoc/>
		public IStoreOperation<PurchaseResult> PurchaseAsync(string productId, object stateObject)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			return PurchaseInternal(StoreOperationType.PurchaseEap, productId,  null, stateObject);
		}

#if UNITYFX_SUPPORT_APM

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginInitialize(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfInitialized();
			ThrowIfInitializePending();

			return InitializeInternal(StoreOperationType.InitializeApm, userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void EndInitialize(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			using (var op = ValidateOperation(asyncResult, StoreOperationType.InitializeApm))
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
			ThrowIfFetchPending();

			return FetchInternal(StoreOperationType.FetchApm, userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void EndFetch(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			using (var op = ValidateOperation(asyncResult, StoreOperationType.FetchApm))
			{
				op.Join();
			}
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IAsyncResult BeginPurchase(string productId, AsyncCallback userCallback, object stateObject)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			return PurchaseInternal(StoreOperationType.PurchaseApm, productId, userCallback, stateObject);
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public PurchaseResult EndPurchase(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();

			using (var op = ValidateOperation(asyncResult, StoreOperationType.PurchaseApm))
			{
				op.Join();
				return (op as PurchaseOperation).ResultUnsafe;
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		/// <inheritdoc/>
		public Task InitializeTaskAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfInitialized();
			ThrowIfInitializePending();

			var tcs = new TaskCompletionSource<object>();
			InitializeInternal(StoreOperationType.InitializeTap, FetchCompletionCallback, tcs);
			return tcs.Task;
		}

		/// <inheritdoc/>
		public Task FetchTaskAsync()
		{
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfNotInitialized();
			ThrowIfFetchPending();

			var tcs = new TaskCompletionSource<object>();
			FetchInternal(StoreOperationType.FetchTap, FetchCompletionCallback, tcs);
			return tcs.Task;
		}

		/// <inheritdoc/>
		public Task<PurchaseResult> PurchaseTaskAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfPlatformNotSupported();
			ThrowIfBusy();

			var tcs = new TaskCompletionSource<PurchaseResult>();
			PurchaseInternal(StoreOperationType.PurchaseTap, productId, PurchaseCompletionCallback, tcs);
			return tcs.Task;
		}

#endif

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

		private StoreOperation InitializeInternal(StoreOperationType opType, AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert((opType & StoreOperationType.Initialize) != 0);

			var result = new InitializeOperation(_storeListener, opType, _purchasingModule, _storeListener, userCallback, stateObject);

			try
			{
				result.Initiate();
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

		private StoreOperation FetchInternal(StoreOperationType opType, AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert((opType & StoreOperationType.Fetch) != 0);

			var result = new FetchOperation(_storeListener, opType, _storeListener.OnFetch, _storeListener.OnFetchFailed, userCallback, stateObject);

			try
			{
				result.Initiate();
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

		private PurchaseOperation PurchaseInternal(StoreOperationType opType, string productId, AsyncCallback userCallback, object stateObject)
		{
			Debug.Assert((opType & StoreOperationType.Purchase) != 0);

			var result = new PurchaseOperation(_storeListener, opType, productId, false, userCallback, stateObject);

			try
			{
				StoreOperation fetchOp = null;

				if (_storeController == null)
				{
					fetchOp = _storeListener.InitializeOp ?? InitializeInternal(StoreOperationType.Initialize, null, null);
				}
				else
				{
					fetchOp = _storeListener.FetchOp;
				}

				if (fetchOp != null)
				{
					fetchOp.AddCompletionHandler(asyncResult =>
					{
						if (!_disposed)
						{
							var op = asyncResult as IStoreOperation;

							if (op.IsCompletedSuccessfully)
							{
								try
								{
									result.Initiate();
								}
								catch (Exception e)
								{
									result.SetFailed(e);
								}
							}
							else
							{
								result.SetFailed(op.Exception);
							}
						}
					});
				}
				else
				{
					result.Initiate();
				}
			}
			catch (Exception e)
			{
				result.SetFailed(e, true);
				throw;
			}

			return result;
		}

		private StoreOperation ValidateOperation(IAsyncResult asyncResult, StoreOperationType type)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException(nameof(asyncResult));
			}

			if (asyncResult is StoreOperation result)
			{
				result.Validate(_storeListener, type);
				return result;
			}
			else
			{
				throw new ArgumentException("Invalid operation type", nameof(asyncResult));
			}
		}

#if UNITYFX_SUPPORT_TAP

		private static void FetchCompletionCallback(IAsyncResult asyncResult)
		{
			var storeOp = asyncResult as IStoreOperation;
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
				tcs.TrySetException(storeOp.Exception);
			}
		}

		private static void PurchaseCompletionCallback(IAsyncResult asyncResult)
		{
			var storeOp = asyncResult as IStoreOperation<PurchaseResult>;
			var tcs = asyncResult.AsyncState as TaskCompletionSource<PurchaseResult>;

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
				tcs.TrySetException(storeOp.Exception);
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

		private void ThrowIfInitializePending()
		{
			if (_storeListener.InitializeOp != null)
			{
				throw new InvalidOperationException(_serviceName + " Initialize is pending.");
			}
		}

		private void ThrowIfFetchPending()
		{
			if (_storeListener.FetchOp != null)
			{
				throw new InvalidOperationException(_serviceName + " Fetch is pending.");
			}
		}

		private static bool IsPlatformSupported()
		{
			return Application.isMobilePlatform || Application.isEditor;
		}

		#endregion
	}
}
