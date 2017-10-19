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
	internal sealed partial class PurchaseService : MonoBehaviour, IStoreService
	{
		#region data

		private const int _traceEventInitialize = 1;
		private const int _traceEventPurchase = 2;

		private const string _serviceName = "Purchasing";

		private const string _errorProductNotAvailable = "Product in not available for purchase";
		private const string _errorManagerNotInitialized = "The manager is not initialized";
		private const string _errorManagerIsBusy = "Another purchase operation is pending";

		private static PurchaseService _instance;

		private Dictionary<string, IStoreProduct> _products = new Dictionary<string, IStoreProduct>();
		private TaskCompletionSource<object> _initializeOpCs;
		private TaskCompletionSource<Product> _purchaseOpCs;
		private bool _disposed;

		private TraceSource _console;
		private IStoreDelegate _delegate;
		private IPurchasingModule _purchasingModule;
		private IStoreController _storeController;

		#endregion

		#region interface

		internal void Initialize(IPurchasingModule purchasingModule, IStoreDelegate storeDelegate)
		{
			_console = new TraceSource(_serviceName);
			_delegate = storeDelegate;
			_purchasingModule = purchasingModule;
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (!ReferenceEquals(_instance, null))
			{
				throw new InvalidOperationException(_serviceName + " instance is already created");
			}

			_instance = this;
		}

		private void OnDisable()
		{
			if (_purchaseOpCs != null)
			{
				InvokePurchaseFailed(null, StorePurchaseError.StoreDisabled, string.Empty);
			}
		}

		private void OnDestroy()
		{
			_products.Clear();
			_purchaseOpCs = null;
			_initializeOpCs = null;
			_instance = null;
		}

		#endregion

		#region IStoreManager

		public async Task InitializeAsync()
		{
			ThrowIfDisposed();
			ThrowIfDisabled();

			if (_storeController == null)
			{
				// Already initialized, do nothing.
			}
			else if (Application.isMobilePlatform || Application.isEditor)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventInitialize, "Initialize");

				try
				{
					_initializeOpCs = new TaskCompletionSource<object>();

					var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
					var storeConfig = await _delegate.GetStoreConfigAsync();

					foreach (var product in storeConfig.Products)
					{
						var productDefinition = product.Definition;
						configurationBuilder.AddProduct(productDefinition.id, productDefinition.type);
						_products.Add(productDefinition.id, product);
					}

					UnityPurchasing.Initialize(this, configurationBuilder);
					await _initializeOpCs.Task;
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, _traceEventPurchase, e);
					_console.TraceEvent(TraceEventType.Stop, _traceEventInitialize, "Initialize failed");
					throw;
				}
				finally
				{
					_initializeOpCs = null;
				}

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

		#endregion

		#region IPlatformStore

		public event EventHandler StoreInitialized;
		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		public IStoreProductCollection Products => this;

		public IStoreController Controller => _storeController;

		public bool IsInitialized => _storeController != null;

		public async Task<Product> PurchaseAsync(string productId)
		{
			ThrowIfInvalidProductId(productId);
			ThrowIfDisposed();
			ThrowIfDisabled();

			if (_purchaseOpCs != null)
			{
				throw new InvalidOperationException(_errorManagerIsBusy);
			}

			using (_delegate.BeginWait())
			{
				// 1) Notify user of the purchase.
				InvokePurchaseInitiated(productId);

				try
				{
					// 2) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController.
					await InitializeAsync();

					// 3) Look up the Product reference with the general product identifier and the Purchasing system's products collection.
					var product = _storeController.products.WithID(productId);

					// 4) If the look up found a product for this device's store and that product is ready to be sold initiate the purchase.
					if (product != null && product.availableToPurchase)
					{
						return await InitiatePurchase(product);
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
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (this && !_disposed)
			{
				_disposed = true;
				Destroy(gameObject);
			}
		}

		#endregion

		#region implementation

		private void ThrowIfDisposed()
		{
			if (_disposed || !this)
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

		private void ThrowIfDisabled()
		{
			if (!isActiveAndEnabled)
			{
				throw new InvalidOperationException(_serviceName + " is disabled");
			}
		}

		#endregion
	}
}
