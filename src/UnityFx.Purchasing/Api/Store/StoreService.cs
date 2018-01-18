// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
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

				SetStoreController(null, null);

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

		internal void SetStoreController(IStoreController controller, IExtensionProvider extensions)
		{
			_storeController = controller;
			_storeExtensions = extensions;
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
				OnPurchaseCompleted(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}

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
		}

		internal void InvokePurchaseFailed(FailedPurchaseResult purchaseResult)
		{
			try
			{
				OnPurchaseFailed(purchaseResult);
			}
			catch (Exception e)
			{
				_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
			}

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
				return _storeListener.IsPurchasePending;
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

			return CompletedStoreOperation.Instance;
		}

		/// <inheritdoc/>
		public IAsyncResult BeginInitialize(AsyncCallback userCallback, object stateObject)
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				return InitializeInternal(userCallback, stateObject);
			}

			return new InitializeOperation(_storeListener, userCallback, stateObject);
		}

		/// <inheritdoc/>
		public void EndInitialize(IAsyncResult asyncResult)
		{
			ThrowIfDisposed();

			var op = ValidateAsyncResult<InitializeOperation>(asyncResult);
			op.Join();
		}

		/// <inheritdoc/>
		public IStoreOperation Fetch()
		{
			ThrowIfDisposed();
			ThrowIfNotInitialized();

			return FetchInternal(null, null);
		}

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

			var op = ValidateAsyncResult<FetchOperation>(asyncResult);
			op.Join();
		}

		/// <inheritdoc/>
		public IStoreOperation<PurchaseResult> Purchase(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			return PurchaseInternal(productId, null, null);
		}

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

			var op = ValidateAsyncResult<PurchaseOperation>(asyncResult);
			return op.Join();
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

		private InitializeOperation InitializeInternal(AsyncCallback userCallback, object stateObject)
		{
			if (_storeListener.IsInitializePending)
			{
				return _storeListener.InitializeOp;
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
					result.SetFailed(e);
					throw;
				}

				return result;
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}

		private FetchOperation FetchInternal(AsyncCallback userCallback, object stateObject)
		{
			if (_storeListener.IsFetchPending)
			{
				return _storeListener.FetchOp;
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
					result.SetFailed(e);
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
				else if (_storeListener.IsFetchPending)
				{
					fetchOp = _storeListener.FetchOp;
				}

				if (fetchOp != null)
				{
					fetchOp.ContinueWith(asyncResult =>
					{
						if (!_disposed)
						{
							if (asyncResult.IsCompletedSuccessfully)
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
								result.SetFailed(asyncResult.Exception);
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
				result.SetFailed(e);
				throw;
			}

			return result;
		}

		private T ValidateAsyncResult<T>(IAsyncResult asyncResult) where T : StoreOperation
		{
			if (asyncResult == null)
			{
				throw new ArgumentNullException(nameof(asyncResult));
			}

			if (asyncResult is T result)
			{
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

		#endregion
	}
}
