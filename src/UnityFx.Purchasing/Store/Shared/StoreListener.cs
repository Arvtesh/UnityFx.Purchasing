// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreListener"/>.
	/// </summary>
	internal sealed class StoreListener : StoreOperationContainer, IStoreListener, IDisposable
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;
		private readonly IPurchasingModule _purchasingModule;

		private InitializeOperation _initializeOp;
		private FetchOperation _fetchOp;
		private PurchaseOperation _purchaseOp;
		private bool _disposed;

		#endregion

		#region interface

		public bool IsInitializePending => _initializeOp != null;

		public AsyncResult<object> InitializeOp => _initializeOp;

		public bool IsFetchPending => _fetchOp != null;

		public AsyncResult<object> FetchOp => _fetchOp;

		public bool IsPurchasePending => _purchaseOp != null;

		public AsyncResult<PurchaseResult> PurchaseOp => _purchaseOp;

		public StoreListener(StoreService storeService, IPurchasingModule purchasingModule)
		{
			_storeService = storeService;
			_console = storeService.TraceSource;
			_purchasingModule = purchasingModule;
		}

		public InitializeOperation BeginInitialize()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			return _initializeOp = new InitializeOperation(this, _purchasingModule, this);
		}

		public FetchOperation BeginFetch()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_purchaseOp == null);

			return _fetchOp = new FetchOperation(this, OnFetch, OnFetchFailed);
		}

		public PurchaseOperation BeginPurchase(string productId, bool restored)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_purchaseOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			return _purchaseOp = new PurchaseOperation(this, productId, restored);
		}

		public PurchaseOperation BeginPurchase(Product product)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_purchaseOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			return _purchaseOp = new PurchaseOperation(this, product, true);
		}

		#endregion

		#region StoreOperationContainer

		public override StoreService Store => _storeService;

		public override void ReleaseOperation(IAsyncOperation op)
		{
			if (op == _initializeOp)
			{
				_initializeOp = null;
			}
			else if (op == _fetchOp)
			{
				_fetchOp = null;
			}
			else if (op == _purchaseOp)
			{
				_purchaseOp = null;
			}
		}

		#endregion

		#region IStoreListener

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Assert(controller != null);
			Debug.Assert(extensions != null);

			if (!_disposed)
			{
				Debug.Assert(_initializeOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Initialize, "OnInitialized");
					_storeService.SetStoreController(controller, extensions);
					_initializeOp.SetCompleted();
				}
				catch (Exception e)
				{
					// Should never get here.
					_initializeOp.SetFailed(StoreFetchError.Unknown, e);
				}
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_initializeOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Initialize, "OnInitializeFailed: " + error);
					_initializeOp.SetFailed(GetInitializeError(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_initializeOp.SetFailed(GetInitializeError(error), e);
				}
			}
		}

		public void OnFetch()
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Fetch, "OnFetch");
					_fetchOp.SetCompleted();
				}
				catch (Exception e)
				{
					// Should never get here.
					_fetchOp.SetFailed(StoreFetchError.Unknown, e);
				}
			}
		}

		public void OnFetchFailed(InitializationFailureReason error)
		{
			if (!_disposed)
			{
				Debug.Assert(_fetchOp != null);

				try
				{
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Fetch, "OnFetchFailed: " + error);
					_fetchOp.SetFailed(GetInitializeError(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_fetchOp.SetFailed(GetInitializeError(error), e);
				}
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			if (!_disposed)
			{
				var op = _purchaseOp;

				try
				{
					var product = args.purchasedProduct;
					var productId = product.definition.id;

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

					if (op == null)
					{
						// A restored transactions when the _purchaseOp is null.
						op = BeginPurchase(product);
						return op.Validate(product);
					}
					else if (op.ProcessPurchase(product))
					{
						// Normal transaction initiated with IStoreService.Purchase()/IStoreService.PurchaseAsync() call.
						return op.Validate(product);
					}
					else
					{
						// Should not really get here. A wierd transaction initiated directly with IStoreController.InitiatePurchase()
						// call (bypassing IStoreService API). Do not process it.
						op = null;
						TraceUnexpectedProduct(productId);
						return PurchaseProcessingResult.Complete;
					}
				}
				catch (Exception e)
				{
					op?.SetFailed(e);
				}
			}

			return PurchaseProcessingResult.Pending;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			if (!_disposed)
			{
				var op = _purchaseOp;

				try
				{
					// NOTE: in some cases product might have null value.
					var productId = product?.definition.id ?? "null";
					var errorDesc = productId + ", reson=" + reason.ToString();

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "OnPurchaseFailed: " + errorDesc);

					if (op == null)
					{
						// A restored transaction.
						if (product != null)
						{
							op = new PurchaseOperation(this, productId, true);
							op.SetFailed(product, GetPurchaseError(reason));
						}
						else
						{
							_console.TraceError(TraceEventId.Purchase, reason.ToString());
						}
					}
					else if (op.IsSame(product))
					{
						// Normal transaction initiated with IStoreService.Purchase()/IStoreService.PurchaseAsync() call.
						op.SetFailed(product, GetPurchaseError(reason));
					}
					else
					{
						// Should not really get here. A wierd transaction initiated directly with IStoreController.InitiatePurchase()
						// call (bypassing IStoreService API).
						op = null;
						TraceUnexpectedProduct(productId);
					}
				}
				catch (Exception e)
				{
					op?.SetFailed(product, GetPurchaseError(reason), e);
				}
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				_purchaseOp?.SetFailed(StorePurchaseError.StoreDisposed);
				_fetchOp?.SetFailed(StoreFetchError.StoreDisposed);
				_initializeOp?.SetFailed(StoreFetchError.StoreDisposed);
			}
		}

		#endregion

		#region implementation

		private void TraceUnexpectedProduct(string productId)
		{
			_console.TraceEvent(TraceEventType.Warning, (int)TraceEventId.Purchase, "Unexpected product: " + productId);
		}

		private static StoreFetchError GetInitializeError(InitializationFailureReason error)
		{
			switch (error)
			{
				case InitializationFailureReason.AppNotKnown:
					return StoreFetchError.AppNotKnown;

				case InitializationFailureReason.NoProductsAvailable:
					return StoreFetchError.NoProductsAvailable;

				case InitializationFailureReason.PurchasingUnavailable:
					return StoreFetchError.PurchasingUnavailable;

				default:
					return StoreFetchError.Unknown;
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

		#endregion
	}
}
