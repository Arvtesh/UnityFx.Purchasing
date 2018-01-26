// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Purchasing;

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

		private InitializeOperation _initializeOp;
		private FetchOperation _fetchOp;
		private List<PurchaseOperation> _purchaseOps;
		private bool _disposed;

		#endregion

		#region interface

		public InitializeOperation InitializeOp => _initializeOp;

		public FetchOperation FetchOp => _fetchOp;

		public bool IsBusy => _purchaseOps.Count > 0;

		public StoreListener(StoreService storeService)
		{
			_storeService = storeService;
			_console = storeService.TraceSource;
			_purchaseOps = new List<PurchaseOperation>();
		}

		public void TryInitiatePurchase(PurchaseOperation op)
		{
			if (_purchaseOps[0] == op)
			{
				op.Initiate();
			}
		}

		#endregion

		#region StoreOperationContainer

		internal override StoreService Store => _storeService;

		internal override void AddOperation(StoreOperation op)
		{
			Debug.Assert(!_disposed);

			if (op is PurchaseOperation pop)
			{
				Debug.Assert(_initializeOp == null);
				Debug.Assert(_fetchOp == null);

				if (_purchaseOps.Count < _storeService.MaxNumberOfPendingPurchases)
				{
					_purchaseOps.Add(pop);
				}
				else
				{
					throw new InvalidOperationException("Maximum number of concurrent purchase operations is exceeded.");
				}
			}
			else if (op is FetchOperation fop)
			{
				Debug.Assert(_initializeOp == null);
				Debug.Assert(_fetchOp == null);

				_fetchOp = fop;
			}
			else if (op is InitializeOperation iop)
			{
				Debug.Assert(_initializeOp == null);
				Debug.Assert(_fetchOp == null);

				_initializeOp = iop;
			}
			else
			{
				Debug.Fail("Unknown operation type");
			}
		}

		internal override void ReleaseOperation(StoreOperation op)
		{
			if (op == _initializeOp)
			{
				_initializeOp = null;
			}
			else if (op == _fetchOp)
			{
				_fetchOp = null;
			}
			else if (op is PurchaseOperation pop)
			{
				_purchaseOps.Remove(pop);

				// Start next purchase operation in queue (if any).
				if (_purchaseOps.Count > 0)
				{
					var popNext = _purchaseOps[0];

					try
					{
						popNext.Initiate();
					}
					catch (Exception e)
					{
						popNext.SetFailed(e);
					}
				}
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
					_console.TraceEvent(TraceEventType.Verbose, _initializeOp.Id, "OnInitialized");
					_storeService.SetStoreController(controller, extensions);
					_initializeOp.SetCompleted();
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _initializeOp.Id, e);
					throw;
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
					_console.TraceEvent(TraceEventType.Verbose, _initializeOp.Id, "OnInitializeFailed: " + error.ToString());
					_initializeOp.SetFailed(GetInitializeError(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _initializeOp.Id, e);
					throw;
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
					_console.TraceEvent(TraceEventType.Verbose, _fetchOp.Id, "OnFetch");
					_fetchOp.SetCompleted();
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _fetchOp.Id, e);
					throw;
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
					_console.TraceEvent(TraceEventType.Verbose, _fetchOp.Id, "OnFetchFailed: " + error.ToString());
					_fetchOp.SetFailed(GetInitializeError(error));
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, _fetchOp.Id, e);
					throw;
				}
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			if (!_disposed)
			{
				var op = _purchaseOps.Count > 0 ? _purchaseOps[0] : null;
				var opId = op?.Id ?? 0;

				try
				{
					var product = args.purchasedProduct;
					var productId = product.definition.id;

					_console.TraceEvent(TraceEventType.Verbose, opId, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, opId, $"Receipt ({productId}): {product.receipt ?? "null"}");

					if (op == null)
					{
						// A restored transactions when the _purchaseOp is null.
						op = new PurchaseOperation(this, product, true);
						return op.Validate();
					}
					else if (op.ProcessPurchase(product))
					{
						// Normal transaction initiated with IStoreService.Purchase()/IStoreService.PurchaseAsync() call.
						return op.Validate();
					}
					else
					{
						// Should not really get here. A wierd transaction initiated directly with IStoreController.InitiatePurchase()
						// call (bypassing IStoreService API). Do not process it.
						TraceUnexpectedProduct(productId, op.ProductId);
						return PurchaseProcessingResult.Complete;
					}
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, opId, e);
					throw;
				}
			}

			return PurchaseProcessingResult.Pending;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			if (!_disposed)
			{
				var op = _purchaseOps.Count > 0 ? _purchaseOps[0] : null;
				var opId = op?.Id ?? 0;

				try
				{
					// NOTE: in some cases product might have null value.
					var productId = product?.definition.id ?? "null";
					var errorDesc = productId + ", reson=" + reason.ToString();

					_console.TraceEvent(TraceEventType.Verbose, opId, "OnPurchaseFailed: " + errorDesc);

					if (op == null)
					{
						// A restored transaction.
						if (product != null)
						{
							op = new PurchaseOperation(this, StoreOperationType.Purchase, productId, true, null, null);
							op.SetFailed(product, GetPurchaseError(reason));
						}
						else
						{
							_console.TraceEvent(TraceEventType.Error, 0, reason.ToString());
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
						TraceUnexpectedProduct(productId, op.ProductId);
					}
				}
				catch (Exception e)
				{
					// Should never get here.
					_console.TraceData(TraceEventType.Critical, opId, e);
					throw;
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

				while (_purchaseOps.Count > 0)
				{
					_purchaseOps[0].SetFailed(StorePurchaseError.StoreDisposed);
				}

				_fetchOp?.SetFailed(StoreFetchError.StoreDisposed);
				_initializeOp?.SetFailed(StoreFetchError.StoreDisposed);
			}
		}

		#endregion

		#region implementation

		private void TraceUnexpectedProduct(string productId, string expectedProductId)
		{
			_console.TraceEvent(TraceEventType.Warning, 0, $"Unexpected product. Got {productId} while {expectedProductId} was expected.");
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
