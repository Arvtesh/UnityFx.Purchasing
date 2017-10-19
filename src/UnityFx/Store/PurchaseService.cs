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

		private const string _errorProductIdInvalid = "Product identifier cannot be null or empty string";
		private const string _errorProductNotAvailable = "Product in not available for purchase";
		private const string _errorManagerDisabled = "The manager is disabled";
		private const string _errorManagerAlreadyInitialized = "The manager is already initialized";
		private const string _errorManagerNotInitialized = "The manager is not initialized";
		private const string _errorManagerIsBusy = "Another purchase operation is pending";
		private const string _errorStoreAlreadyExists = "IStoreManager instance is already created";

		private static PurchaseService _instance;

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
				throw new InvalidOperationException(_errorStoreAlreadyExists);
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
			_purchaseOpCs = null;
			_initializeOpCs = null;

			_storeController = null;
			_purchasingModule = null;
			_delegate = null;
			_console = null;

			_instance = null;
		}

		#endregion

		#region IStoreManager

		public Task InitializeAsync(IEnumerable<ProductDefinition> items)
		{
			ThrowIfDisposed();

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			if (!isActiveAndEnabled)
			{
				throw new InvalidOperationException(_errorManagerDisabled);
			}

			if (_storeController != null)
			{
				throw new InvalidOperationException(_errorManagerAlreadyInitialized);
			}

			if (Application.isMobilePlatform || Application.isEditor)
			{
				_console.TraceEvent(TraceEventType.Start, _traceEventInitialize, "Initialize");

				var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
				configurationBuilder.AddProducts(items);
				UnityPurchasing.Initialize(this, configurationBuilder);

				_initializeOpCs = new TaskCompletionSource<object>();
				return _initializeOpCs.Task;
			}

			return Task.CompletedTask;
		}

		#endregion

		#region IPlatformStore

		public event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;
		public event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;
		public event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		public IStoreProductCollection Products => this;

		public IStoreController Controller => _storeController;

		public bool IsInitialized => _storeController != null;

		public async Task<Product> PurchaseAsync(string productId)
		{
			ThrowIfDisposed();

			if (string.IsNullOrEmpty(productId))
			{
				throw new ArgumentException(_errorProductIdInvalid, nameof(productId));
			}

			if (!isActiveAndEnabled)
			{
				throw new InvalidOperationException(_errorManagerDisabled);
			}

			if (_storeController == null && _initializeOpCs == null)
			{
				throw new InvalidOperationException(_errorManagerNotInitialized);
			}

			if (_purchaseOpCs != null)
			{
				throw new InvalidOperationException(_errorManagerIsBusy);
			}

			using (_delegate?.BeginWait())
			{
				// 1) Notify user of the purchase.
				InvokePurchaseInitiated(productId);

				try
				{
					// 2) Wait untill the store initialization is finished. If the initialization fails for any reason
					// an exception will be thrown, so no need to null-check _storeController again.
					await _initializeOpCs?.Task;

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
					InvokePurchaseFailed(e.Product, e.Reason, e.StoreId);
					throw;
				}
				catch (StoreInitializeException e)
				{
					InvokePurchaseFailed(null, StorePurchaseError.StoreInitializationFailed, null);
					throw new StorePurchaseException(StorePurchaseError.StoreInitializationFailed, null, null, e);
				}
				catch (Exception e)
				{
					InvokePurchaseFailed(null, StorePurchaseError.Unknown, null);
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

		#endregion
	}
}
