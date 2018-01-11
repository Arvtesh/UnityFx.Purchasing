// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates possible initialization errors.
	/// </summary>
	public enum StoreFetchError
	{
		/// <summary>
		/// A catch-all for unrecognized fetch/initialize problems.
		/// </summary>
		Unknown,

		/// <summary>
		/// The manager was disposed while a fetch/initialize operation was pending.
		/// </summary>
		StoreDisposed,

		/// <summary>
		/// In-App Purchases disabled in device settings (<see cref="InitializationFailureReason.PurchasingUnavailable"/>).
		/// </summary>
		PurchasingUnavailable,

		/// <summary>
		/// No products available for purchase (<see cref="InitializationFailureReason.NoProductsAvailable"/>).
		/// </summary>
		NoProductsAvailable,

		/// <summary>
		/// The store reported the app as unknown (<see cref="InitializationFailureReason.AppNotKnown"/>).
		/// </summary>
		AppNotKnown
	}

	/// <summary>
	/// Enumerates possible purchase errors.
	/// </summary>
	public enum StorePurchaseError
	{
		/// <summary>
		/// A catch-all for unrecognized purchase problems (<see cref="PurchaseFailureReason.Unknown"/>).
		/// </summary>
		Unknown,

		/// <summary>
		/// The manager was disposed while a purchase operation was pending.
		/// </summary>
		StoreDisposed,

		/// <summary>
		/// The system purchasing feature is unavailable (<see cref="PurchaseFailureReason.PurchasingUnavailable"/>).
		/// </summary>
		PurchasingUnavailable,

		/// <summary>
		/// A purchase was already in progress when a new purchase was requested (<see cref="PurchaseFailureReason.ExistingPurchasePending"/>).
		/// </summary>
		ExistingPurchasePending,

		/// <summary>
		/// The product is not available to purchase on the store (<see cref="PurchaseFailureReason.ProductUnavailable"/>).
		/// </summary>
		ProductUnavailable,

		/// <summary>
		/// Signature validation of the purchase's receipt failed (<see cref="PurchaseFailureReason.SignatureInvalid"/>).
		/// </summary>
		SignatureInvalid,

		/// <summary>
		/// The user opted to cancel rather than proceed with the purchase (<see cref="PurchaseFailureReason.UserCancelled"/>).
		/// </summary>
		UserCanceled,

		/// <summary>
		/// There was a problem with the payment (<see cref="PurchaseFailureReason.PaymentDeclined"/>).
		/// </summary>
		PaymentDeclined,

		/// <summary>
		/// A duplicate transaction error when the transaction has already been completed successfully (<see cref="PurchaseFailureReason.DuplicateTransaction"/>).
		/// </summary>
		DuplicateTransaction,

		/// <summary>
		/// Purchase receipt is null or an empty string.
		/// </summary>
		ReceiptNullOrEmpty,

		/// <summary>
		/// Store validation of purchase receipt failed.
		/// </summary>
		ReceiptValidationFailed,

		/// <summary>
		/// Store validation of purchase receipt not available.
		/// </summary>
		ReceiptValidationNotAvailable
	}

	/// <summary>
	/// A generic platform store service.
	/// </summary>
	public interface IStoreService : IStoreEvents, IDisposable
	{
		/// <summary>
		/// Returns store products list. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		IStoreProductCollection Products { get; }

		/// <summary>
		/// Returns the service settings. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		IStoreServiceSettings Settings { get; }

		/// <summary>
		/// Returns Unity3d store controller. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		IStoreController Controller { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store is initialized (the product list is loaded from native store); <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		bool IsInitialized { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store has pending purchase operation; <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		bool IsBusy { get; }

#if NET35
		/// <summary>
		/// Initializes the store. Does nothing (returns a completed task) if already initialized.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if store initialization fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		AsyncResult Initialize();
#else
		/// <summary>
		/// Initializes the store. Does nothing (returns a completed task) if already initialized.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if store initialization fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		Task InitializeAsync();
#endif

#if NET35
		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if operation fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		AsyncResult Fetch();
#else
		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if operation fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		Task FetchAsync();
#endif

#if NET35
		/// <summary>
		/// Initiates purchasing the specified product.
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="productId">Product identifier as specified in the store.</param>
		/// <exception cref="StorePurchaseException">Thrown if an purchase-related errors.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		AsyncResult<PurchaseResult> Purchase(string productId);
#else
		/// <summary>
		/// Initiates purchasing the specified product.
		/// </summary>
		/// <remarks>
		/// If <see cref="InitializeAsync"/> or <see cref="FetchAsync"/> is in progress, waits for them to complete before proceed.
		/// If the store is not initialized yet calls <see cref="InitializeAsync"/> first.
		/// </remarks>
		/// <param name="productId">Product identifier as specified in the store.</param>
		/// <exception cref="StorePurchaseException">Thrown if an purchase-related errors.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		Task<PurchaseResult> PurchaseAsync(string productId);
#endif
	}
}
