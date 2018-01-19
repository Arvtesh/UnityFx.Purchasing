// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	using Debug = System.Diagnostics.Debug;

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
	/// <seealso cref="IStoreService"/>
	public abstract class StoreService : IStoreService, IStoreServiceSettings
	{
		#region data

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly StoreListener _storeListener;
		private readonly IPurchasingModule _purchasingModule;

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
		/// Identifier for user trace events.
		/// </summary>
		protected const int TraceEventMax = 4;

		/// <summary>
		/// Returns the <see cref="System.Diagnostics.TraceSource"/> instance used by the service. Read only.
		/// </summary>
		protected internal TraceSource TraceSource => _console;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreService"/> class.
		/// </summary>
		protected StoreService(string name, IPurchasingModule purchasingModule)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_purchasingModule = purchasingModule;
			_storeListener = new StoreListener(this);
		}

#if UNITYFX_SUPPORT_TAP

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		/// <remarks>
		/// Typlical implementation would connect to the app server for information on products available.
		/// </remarks>
		/// <seealso cref="ValidatePurchaseAsync(StoreTransaction)"/>
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
		/// <param name="transaction">The transaction data to validate.</param>
		/// <seealso cref="GetStoreConfigAsync()"/>
		protected internal virtual Task<PurchaseValidationResult> ValidatePurchaseAsync(StoreTransaction transaction)
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
		/// <param name="completedDelegate">Operation completed delegate.</param>
		/// <param name="failedDelegate">Delegate called on operation failure.</param>
		/// <seealso cref="ValidatePurchase(StoreTransaction, Action{PurchaseValidationResult})"/>
		protected internal abstract void GetStoreConfig(Action<StoreConfig> completedDelegate, Action<Exception> failedDelegate);

		/// <summary>
		/// Validates a purchase. Inherited classes may override this method if purchase validation is required.
		/// Default implementation just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// Typical implementation would first do client validation of the purchase and (if that passes) initiate server-side validation.
		/// </remarks>
		/// <returns>Returns <see langword="true"/> if validation is implemented; <see langword="false"/> if not.</returns>
		/// <param name="transaction">The transaction data to validate.</param>
		/// <param name="resultDelegate">Operation completed delegate.</param>
		/// <seealso cref="GetStoreConfig(Action{StoreConfig}, Action{Exception})"/>
		protected internal virtual bool ValidatePurchase(StoreTransaction transaction, Action<PurchaseValidationResult> resultDelegate)
		{
			return false;
		}

#endif

		/// <summary>
		/// Called when the store initialize operation has been initiated.
		/// </summary>
		/// <seealso cref="OnInitializeCompleted(IStoreOperation)"/>
		/// <seealso cref="OnInitializeFailed(IStoreOperation, StoreFetchError, Exception)"/>
		protected virtual void OnInitializeInitiated(IStoreOperation op)
		{
			InitializeInitiated?.Invoke(this, new FetchInitiatedEventArgs(op));
		}

		/// <summary>
		/// Called when the store initialization has succeeded.
		/// </summary>
		/// <seealso cref="OnInitializeFailed(IStoreOperation, StoreFetchError, Exception)"/>
		/// <seealso cref="OnInitializeInitiated"/>
		protected virtual void OnInitializeCompleted(IStoreOperation op)
		{
			InitializeCompleted?.Invoke(this, new FetchCompletedEventArgs(op));
		}

		/// <summary>
		/// Called when the store initialization has failed.
		/// </summary>
		/// <seealso cref="OnInitializeCompleted(IStoreOperation)"/>
		/// <seealso cref="OnInitializeInitiated(IStoreOperation)"/>
		protected virtual void OnInitializeFailed(IStoreOperation op, StoreFetchError reason, Exception e)
		{
			InitializeCompleted?.Invoke(this, new FetchCompletedEventArgs(op, reason, e));
		}

		/// <summary>
		/// Called when the store fetch operation has been initiated.
		/// </summary>
		/// <seealso cref="OnFetchCompleted(IStoreOperation)"/>
		/// <seealso cref="OnFetchFailed(IStoreOperation, StoreFetchError, Exception)"/>
		protected virtual void OnFetchInitiated(IStoreOperation op)
		{
			FetchInitiated?.Invoke(this, new FetchInitiatedEventArgs(op));
		}

		/// <summary>
		/// Called when the store fetch has succeeded.
		/// </summary>
		/// <seealso cref="OnFetchFailed(IStoreOperation, StoreFetchError, Exception)"/>
		/// <seealso cref="OnFetchInitiated(IStoreOperation)"/>
		protected virtual void OnFetchCompleted(IStoreOperation op)
		{
			FetchCompleted?.Invoke(this, new FetchCompletedEventArgs(op));
		}

		/// <summary>
		/// Called when the store fetch has failed.
		/// </summary>
		/// <seealso cref="OnFetchCompleted(IStoreOperation)"/>
		/// <seealso cref="OnFetchInitiated(IStoreOperation)"/>
		protected virtual void OnFetchFailed(IStoreOperation op, StoreFetchError reason, Exception e)
		{
			FetchCompleted?.Invoke(this, new FetchCompletedEventArgs(op, reason, e));
		}

		/// <summary>
		/// Called when the store purchase operation has been initiated.
		/// </summary>
		/// <seealso cref="OnPurchaseCompleted(IStoreOperation, PurchaseResult)"/>
		/// <seealso cref="OnPurchaseFailed(IStoreOperation, FailedPurchaseResult)"/>
		protected virtual void OnPurchaseInitiated(IStoreOperation op, string productId, bool isRestored)
		{
			PurchaseInitiated?.Invoke(this, new PurchaseInitiatedEventArgs(op, productId, isRestored));
		}

		/// <summary>
		/// Called when the store purchase operation succeded.
		/// </summary>
		/// <seealso cref="OnPurchaseFailed(IStoreOperation, FailedPurchaseResult)"/>
		/// <seealso cref="OnPurchaseInitiated(IStoreOperation, string, bool)"/>
		protected virtual void OnPurchaseCompleted(IStoreOperation op, PurchaseResult result)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(op, result));
		}

		/// <summary>
		/// Called when the store purchase operation has failed.
		/// </summary>
		/// <seealso cref="OnPurchaseCompleted(IStoreOperation, PurchaseResult)"/>
		/// <seealso cref="OnPurchaseInitiated(IStoreOperation, string, bool)"/>
		protected virtual void OnPurchaseFailed(IStoreOperation op, FailedPurchaseResult result)
		{
			PurchaseCompleted?.Invoke(this, new PurchaseCompletedEventArgs(op, result));
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
			if (_storeListener.PurchaseOp != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
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

		internal void InvokeInitializeInitiated(IStoreOperation op)
		{
			try
			{
				OnInitializeInitiated(op);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Initialize, e);
			}
		}

		internal void InvokeInitializeCompleted(IStoreOperation op)
		{
			try
			{
				OnInitializeCompleted(op);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Initialize, e);
			}
		}

		internal void InvokeInitializeFailed(IStoreOperation op, StoreFetchError reason, Exception ex)
		{
			try
			{
				OnInitializeFailed(op, reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Initialize, e);
			}
		}

		internal void InvokeFetchInitiated(IStoreOperation op)
		{
			try
			{
				OnFetchInitiated(op);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Fetch, e);
			}
		}

		internal void InvokeFetchCompleted(IStoreOperation op)
		{
			try
			{
				OnInitializeCompleted(op);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Fetch, e);
			}
		}

		internal void InvokeFetchFailed(IStoreOperation op, StoreFetchError reason, Exception ex)
		{
			try
			{
				OnInitializeFailed(op, reason, ex);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Fetch, e);
			}
		}

		internal void InvokePurchaseInitiated(IStoreOperation op, string productId, bool restored)
		{
			Debug.Assert(!string.IsNullOrEmpty(productId));

			try
			{
				OnPurchaseInitiated(op, productId, restored);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
			}
		}

		internal void InvokePurchaseCompleted(IStoreOperation op, string productId, PurchaseResult purchaseResult)
		{
			Debug.Assert(purchaseResult != null);

			try
			{
				OnPurchaseCompleted(op, purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
			}

#if UNITYFX_SUPPORT_OBSERVABLES

			try
			{
				_purchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
			}

#endif
		}

		internal void InvokePurchaseFailed(IStoreOperation op, FailedPurchaseResult purchaseResult)
		{
			try
			{
				OnPurchaseFailed(op, purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
			}

#if UNITYFX_SUPPORT_OBSERVABLES

			try
			{
				_failedPurchases?.OnNext(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)StoreOperationId.Purchase, e);
			}

#endif
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
				ThrowIfNotInitialized();
				return _storeController;
			}
		}

		/// <inheritdoc/>
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
		public IStoreOperation Initialize()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				return InitializeInternal(null, null);
			}

			return StoreOperation.GetCompletedOperation(_storeListener, StoreOperationId.Initialize, null, null);
		}

#if UNITYFX_SUPPORT_APM

		/// <inheritdoc/>
		public IAsyncResult BeginInitialize(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				return InitializeInternal(userCallback, stateObject);
			}

			return StoreOperation.GetCompletedOperation(_storeListener, StoreOperationId.Initialize, userCallback, stateObject);
		}

		/// <inheritdoc/>
		public void EndInitialize(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation(asyncResult, StoreOperationId.Initialize))
			{
				op.Join();
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		/// <inheritdoc/>
		public Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				var tcs = new TaskCompletionSource<object>();
				InitializeInternal(FetchCompletionCallback, tcs);
				return tcs.Task;
			}

			return Task.CompletedTask;
		}

#endif

		/// <inheritdoc/>
		public IStoreOperation Fetch()
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();

			return FetchInternal(null, null);
		}

#if UNITYFX_SUPPORT_APM

		/// <inheritdoc/>
		public IAsyncResult BeginFetch(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();

			return FetchInternal(userCallback, stateObject);
		}

		/// <inheritdoc/>
		public void EndFetch(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation(asyncResult, StoreOperationId.Fetch))
			{
				op.Join();
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		/// <inheritdoc/>
		public Task FetchAsync()
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();

			var tcs = new TaskCompletionSource<object>();
			FetchInternal(FetchCompletionCallback, tcs);
			return tcs.Task;
		}

#endif

		/// <inheritdoc/>
		public IStoreOperation<PurchaseResult> Purchase(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			return PurchaseInternal(productId, null, null);
		}

#if UNITYFX_SUPPORT_APM

		/// <inheritdoc/>
		public IAsyncResult BeginPurchase(string productId, AsyncCallback userCallback, object stateObject)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			return PurchaseInternal(productId, userCallback, stateObject);
		}

		/// <inheritdoc/>
		public PurchaseResult EndPurchase(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			using (var op = ValidateOperation<PurchaseResult>(asyncResult, StoreOperationId.Purchase))
			{
				return op.Join();
			}
		}

#endif

#if UNITYFX_SUPPORT_TAP

		/// <inheritdoc/>
		public Task<PurchaseResult> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			var tcs = new TaskCompletionSource<PurchaseResult>();
			PurchaseInternal(productId, PurchaseCompletionCallback, tcs);
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

		#region IStoreEvents

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

		private StoreOperation InitializeInternal(AsyncCallback userCallback, object stateObject)
		{
			var op = _storeListener.InitializeOp;

			if (op != null)
			{
				return op.ContinueWith(userCallback, stateObject);
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				var result = new InitializeOperation(_storeListener, _purchasingModule, _storeListener, userCallback, stateObject);

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
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

		private StoreOperation FetchInternal(AsyncCallback userCallback, object stateObject)
		{
			var op = _storeListener.FetchOp;

			if (op != null)
			{
				return op.ContinueWith(userCallback, stateObject);
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				var result = new FetchOperation(_storeListener, _storeListener.OnFetch, _storeListener.OnFetchFailed, userCallback, stateObject);

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
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

		private PurchaseOperation PurchaseInternal(string productId, AsyncCallback userCallback, object stateObject)
		{
			var result = new PurchaseOperation(_storeListener, productId, false, userCallback, stateObject);

			try
			{
				StoreOperation fetchOp = null;

				if (_storeController == null)
				{
					fetchOp = InitializeInternal(null, null);
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

		private IStoreOperation ValidateOperation(IAsyncResult asyncResult, StoreOperationId type)
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException(nameof(asyncResult));
			}

			if (asyncResult is StoreOperation result)
			{
				if (result.Type != type)
				{
					throw new ArgumentException("Invalid operation type", nameof(asyncResult));
				}

				if (result.Owner != _storeListener)
				{
					throw new InvalidOperationException("Invalid operation owner");
				}

				return result;
			}
			else
			{
				throw new ArgumentException("Invalid operation type", nameof(asyncResult));
			}
		}

		private IStoreOperation<T> ValidateOperation<T>(IAsyncResult asyncResult, StoreOperationId type)
		{
			return ValidateOperation(asyncResult, type) as IStoreOperation<T>;
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

		#endregion
	}
}
