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
	/// <summary>
	/// Implementation of <see cref="IStoreService"/>.
	/// </summary>
	internal sealed partial class StoreService : IStoreService
	{
		#region data

		private const int _traceEventInitialize = 1;
		private const int _traceEventPurchase = 2;

		private readonly string _serviceName;
		private readonly TraceSource _console;
		private readonly IStoreDelegate _delegate;
		private readonly IPurchasingModule _purchasingModule;

		private Dictionary<string, IStoreProduct> _products = new Dictionary<string, IStoreProduct>();
		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<PurchaseResult> _purchaseOpCs;
		private IStoreController _storeController;
		private bool _disposed;

		#endregion

		#region interface

		internal StoreService(string name, IPurchasingModule purchasingModule, IStoreDelegate storeDelegate)
		{
			_serviceName = string.IsNullOrEmpty(name) ? "Purchasing" : "Purchasing." + name;
			_console = new TraceSource(_serviceName);
			_delegate = storeDelegate;
			_purchasingModule = purchasingModule;
		}

		#endregion

		#region IStoreService
		#endregion

		#region IPlatformStore

		public event EventHandler StoreInitialized;
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		public IObservable<StoreTransaction> Transactions
		{
			get
			{
				ThrowIfDisposed();
				throw new NotImplementedException();
			}
		}

		public IStoreProductCollection Products
		{
			get
			{
				ThrowIfDisposed();
				return this;
			}
		}

		public IStoreController Controller
		{
			get
			{
				ThrowIfDisposed();
				return _storeController;
			}
		}

		public bool IsInitialized
		{
			get
			{
				ThrowIfDisposed();
				return _storeController != null;
			}
		}

		public bool IsBusy
		{
			get
			{
				ThrowIfDisposed();
				return _purchaseOpCs != null;
			}
		}

		public async Task InitializeAsync()
		{
			ThrowIfDisposed();

			if (_storeController == null)
			{
				// Already initialized, do nothing.
			}
			else if (_initializeOpCs != null)
			{
				// Initialization is pending.
				await _initializeOpCs?.Task;
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventInitialize, "Initialize");

				try
				{
					_initializeOpCs = new TaskCompletionSource<object>();

					// 1) Get store configuration. Should be provided by the service user.
					var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
					var storeConfig = await _delegate.GetStoreConfigAsync();

					// 2) Initialize the store.This connects to real store and retrieves information on products specified in the previous step.
					foreach (var product in storeConfig.Products)
					{
						var productDefinition = product.Definition;
						configurationBuilder.AddProduct(productDefinition.id, productDefinition.type);
						_products.Add(productDefinition.id, product);
					}

					UnityPurchasing.Initialize(this, configurationBuilder);
					await _initializeOpCs.Task;

					// 3) Initialize metadata in _products.
					foreach (var product in _storeController.products.all)
					{
						_products[product.definition.id].Metadata = product.metadata;
					}
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventInitialize, e);
					_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize failed");
					throw;
				}
				finally
				{
					_initializeOpCs = null;
				}

				// Trigger user-defined events.
				try
				{
					StoreInitialized?.Invoke(this, EventArgs.Empty);
				}
				finally
				{
					_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize complete");
				}
			}
		}

		public async Task<StoreTransaction> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfBusy();

			// 1) Turn on user-defined wait animation (if any).
			using (_delegate.BeginWait())
			{
				// 2) Notify user of the purchase.
				InvokePurchaseInitiated(productId);

				try
				{
					// 3) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 4) Look up the Product reference with the general product identifier and the Purchasing system's products collection.
					var product = _storeController.products.WithID(productId);

					// 5) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
					if (product != null && product.availableToPurchase)
					{
						_console.TraceEvent(TraceEventType.Information, _traceEventPurchase, $"InitiatePurchase: {product.definition.id} ({product.definition.storeSpecificId}), type={product.definition.type}, price={product.metadata.localizedPriceString}");
						_purchaseOpCs = new TaskCompletionSource<PurchaseResult>(product);
						_storeController.InitiatePurchase(product);

						// 6) Wait for the purchase and validation process to complete, notify users and return.
						var purchaseResult = await _purchaseOpCs.Task;
						InvokePurchaseCompleted(purchaseResult);
						return purchaseResult.TransactionInfo;
					}
					else
					{
						throw new StorePurchaseException(StorePurchaseError.ProductUnavailable, product, null);
					}
				}
				catch (StorePurchaseException e)
				{
					InvokePurchaseFailed(e.Product, e.Reason, e.StoreId, e);
					throw;
				}
				catch (StoreInitializeException e)
				{
					InvokePurchaseFailed(null, StorePurchaseError.StoreInitializationFailed, null, e);
					throw new StorePurchaseException(StorePurchaseError.StoreInitializationFailed, null, null, e);
				}
				catch (Exception e)
				{
					InvokePurchaseFailed(null, StorePurchaseError.Unknown, null, e);
					throw new StorePurchaseException(StorePurchaseError.Unknown, null, null, e);
				}
				finally
				{
					_purchaseOpCs = null;
				}
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_products.Clear();
				_purchaseOpCs = null;
				_initializeOpCs = null;
				_disposed = true;
			}
		}

		#endregion

		#region implementation

		private void ThrowIfDisposed()
		{
			if (_disposed)
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
			if (_purchaseOpCs != null)
			{
				throw new InvalidOperationException(_serviceName + " is busy");
			}
		}

		#endregion
	}
}
