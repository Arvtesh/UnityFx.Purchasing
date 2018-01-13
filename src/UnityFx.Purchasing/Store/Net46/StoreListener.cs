// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreListener"/>.
	/// </summary>
	internal sealed class StoreListener : IStoreListener, IDisposable
	{
		#region data

		private readonly StoreService _storeService;
		private readonly TraceSource _console;

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

		public StoreListener(StoreService storeService, TraceSource console)
		{
			_storeService = storeService;
			_console = console;
		}

		public InitializeOperation BeginInitialize()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			_initializeOp = new InitializeOperation(_console);
			_storeService.InvokeInitializeInitiated();

			return _initializeOp;
		}

		public void EndInitialize(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);

			if (_initializeOp != null)
			{
				if (e is StoreFetchException sfe)
				{
					_storeService.InvokeInitializeFailed(GetInitializeError(sfe.Reason), e);
				}
				else
				{
					_storeService.InvokeInitializeFailed(StoreFetchError.Unknown, e);
				}

				_initializeOp.TrySetException(e);
				_initializeOp.Dispose();
				_initializeOp = null;
			}
		}

		public FetchOperation BeginFetch()
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_fetchOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_purchaseOp == null);

			_fetchOp = new FetchOperation(_console);
			_storeService.InvokeFetchInitiated();

			return _fetchOp;
		}

		public void EndFetch(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);

			if (_fetchOp != null)
			{
				if (e is StoreFetchException sfe)
				{
					_storeService.InvokeFetchFailed(GetInitializeError(sfe.Reason), e);
				}
				else
				{
					_storeService.InvokeFetchFailed(StoreFetchError.Unknown, e);
				}

				_fetchOp.TrySetException(e);
				_fetchOp.Dispose();
				_fetchOp = null;
			}
		}

		public PurchaseOperation BeginPurchase(string productId, bool isRestored)
		{
			Debug.Assert(!_disposed);
			Debug.Assert(_purchaseOp == null);
			Debug.Assert(_initializeOp == null);
			Debug.Assert(_fetchOp == null);

			_purchaseOp = new PurchaseOperation(_storeService, _console, productId, isRestored);
			_storeService.InvokePurchaseInitiated(productId, isRestored);

			return _purchaseOp;
		}

		public void EndPurchase(Exception e)
		{
			Debug.Assert(!_disposed);

			_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);

			if (_purchaseOp != null)
			{
				if (e is StorePurchaseException spe)
				{
					_storeService.InvokePurchaseFailed(new FailedPurchaseResult(spe));
				}
				else if (e is StoreFetchException sfe)
				{
					_storeService.InvokePurchaseFailed(_purchaseOp.GetFailedResult(StorePurchaseError.StoreNotInitialized, e));
				}
				else
				{
					_storeService.InvokePurchaseFailed(_purchaseOp.GetFailedResult(StorePurchaseError.Unknown, e));
				}

				_purchaseOp.TrySetException(e);
				_purchaseOp.Dispose();
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
					_storeService.InvokeInitializeCompleted();
					_initializeOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
					_initializeOp.TrySetException(e);
				}
				finally
				{
					_initializeOp.Dispose();
					_initializeOp = null;
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
					var e = new StoreFetchException(error);

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Initialize, "OnInitializeFailed: " + error);
					_storeService.InvokeInitializeFailed(GetInitializeError(error), e);
					_initializeOp.SetException(e);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Initialize, e);
					_initializeOp.TrySetException(e);
				}
				finally
				{
					_initializeOp.Dispose();
					_initializeOp = null;
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
					_storeService.InvokeFetchCompleted();
					_fetchOp.SetResult(null);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
					_fetchOp.TrySetException(e);
				}
				finally
				{
					_fetchOp.Dispose();
					_fetchOp = null;
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
					var e = new StoreFetchException(error);

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Fetch, "OnFetchFailed: " + error);
					_storeService.InvokeFetchFailed(GetInitializeError(error), e);
					_fetchOp.SetException(e);
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Fetch, e);
					_fetchOp.TrySetException(e);
				}
				finally
				{
					_fetchOp.Dispose();
					_fetchOp = null;
				}
			}
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
			Debug.Assert(args != null);
			Debug.Assert(args.purchasedProduct != null);

			if (!_disposed)
			{
				try
				{
					var result = PurchaseProcessingResult.Complete;
					var product = args.purchasedProduct;
					var productId = product.definition.id;

					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ProcessPurchase: " + productId);
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"Receipt ({productId}): {product.receipt ?? "null"}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = BeginPurchase(productId, true);
					}

					// Validate the purchase transaction.
					if (_purchaseOp.ProcessPurchase(product))
					{
						var transactionInfo = _purchaseOp.Transaction;

						_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"ValidatePurchase: {productId}, transactionId = {product.transactionID}");

						if (string.IsNullOrEmpty(transactionInfo.Receipt))
						{
							_purchaseOp.SetPurchaseFailed(StorePurchaseError.ReceiptNullOrEmpty);
						}
						else if (_storeService.ValidatePurchase(transactionInfo, ValidatePurchaseCallback))
						{
							// Check if the validation callback was called synchronously.
							if (!_purchaseOp.IsCompleted)
							{
								result = PurchaseProcessingResult.Pending;
							}
						}
						else
						{
							_purchaseOp.SetPurchaseCompleted(new PurchaseValidationResult(PurchaseValidationStatus.Suppressed));
						}
					}

					// Release the operation if done.
					if (result == PurchaseProcessingResult.Complete)
					{
						_purchaseOp.Dispose();
						_purchaseOp = null;
					}

					return result;
				}
				catch (Exception e)
				{
					EndPurchase(e);
				}
			}

			return PurchaseProcessingResult.Pending;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
		{
			if (!_disposed)
			{
				try
				{
					// NOTE: in some cases product might have null value.
					var productId = product?.definition.id ?? "null";
					_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, $"OnPurchaseFailed: {productId}, reason={reason}");

					// Handle restored transactions when the _purchaseOp is not initialized.
					if (_purchaseOp == null)
					{
						_purchaseOp = BeginPurchase(productId, true);
					}

					_purchaseOp.SetPurchaseFailed(product, GetPurchaseError(reason));
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_purchaseOp?.Dispose();
					_purchaseOp = null;
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

				if (_purchaseOp != null)
				{
					_storeService.InvokePurchaseFailed(new FailedPurchaseResult(_purchaseOp.ProductId, null, null, StorePurchaseError.StoreDisposed, null));
					_purchaseOp.Dispose();
					_purchaseOp = null;
				}

				if (_fetchOp != null)
				{
					_storeService.InvokeFetchFailed(StoreFetchError.StoreDisposed, null);
					_fetchOp.Dispose();
					_fetchOp = null;
				}

				if (_initializeOp != null)
				{
					_storeService.InvokeInitializeFailed(StoreFetchError.StoreDisposed, null);
					_initializeOp.Dispose();
					_initializeOp = null;
				}
			}
		}

		#endregion

		#region implementation

		private void ValidatePurchaseCallback(PurchaseValidationResult validationResult)
		{
			if (!_disposed)
			{
				Debug.Assert(_purchaseOp != null);

				try
				{
					if (validationResult == null)
					{
						validationResult = new PurchaseValidationResult(PurchaseValidationStatus.Suppressed);
					}

					var resultStatus = validationResult.Status;
					var transactionInfo = _purchaseOp.Transaction;
					var product = transactionInfo.Product;

					if (resultStatus == PurchaseValidationStatus.Ok || resultStatus == PurchaseValidationStatus.Suppressed)
					{
						// The purchase validation succeeded.
						ConfirmPendingPurchase(product);
						_purchaseOp.SetPurchaseCompleted(validationResult);
					}
					else if (resultStatus == PurchaseValidationStatus.Failure)
					{
						// The purchase validation failed: confirm to avoid processing it again.
						ConfirmPendingPurchase(product);
						_purchaseOp.SetPurchaseFailed(validationResult, StorePurchaseError.ReceiptValidationFailed, null);
					}
					else
					{
						// Need to re-validate the purchase: do not confirm.
						_purchaseOp.SetPurchaseFailed(validationResult, StorePurchaseError.ReceiptValidationNotAvailable, null);
					}
				}
				catch (Exception e)
				{
					_console.TraceData(TraceEventType.Error, (int)TraceEventId.Purchase, e);
					_purchaseOp?.TrySetException(e);
				}
				finally
				{
					_purchaseOp.Dispose();
					_purchaseOp = null;
				}
			}
		}

		private void ConfirmPendingPurchase(Product product)
		{
			_console.TraceEvent(TraceEventType.Verbose, (int)TraceEventId.Purchase, "ConfirmPendingPurchase: " + product.definition.id);
			_storeService.Controller.ConfirmPendingPurchase(product);
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
