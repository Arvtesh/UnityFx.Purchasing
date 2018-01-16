// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
#if !NET35
using System.Threading.Tasks;
#endif
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
		/// The store was disposed while a purchase operation was pending.
		/// </summary>
		StoreDisposed,

		/// <summary>
		/// The store is not initialized.
		/// </summary>
		StoreNotInitialized,

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
		/// Store local validation of purchase receipt failed.
		/// </summary>
		ReceiptLocalValidationFailed,

		/// <summary>
		/// Store validation (either local or not) of purchase receipt failed.
		/// </summary>
		ReceiptValidationFailed,

		/// <summary>
		/// Store validation of purchase receipt not available.
		/// </summary>
		ReceiptValidationNotAvailable
	}

	/// <summary>
	/// A generic in-app store service.
	/// </summary>
	/// <remarks>
	/// The interface defines a wrapper around <see href="https://docs.unity3d.com/Manual/UnityIAP.html">Unity IAP</see>.
	/// </remarks>
	/// <seealso href="https://docs.unity3d.com/Manual/UnityIAP.html"/>
	/// <seealso cref="IStoreController"/>
	/// <seealso cref="IExtensionProvider"/>
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
		/// Returns Unity3d store controller. Never returns <see langword="null"/>. Read only.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the store is not initialized.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="IsInitialized"/>
		IStoreController Controller { get; }

		/// <summary>
		/// Returns store extensions provider. Never returns <see langword="null"/>. Read only.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the store is not initialized.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="IsInitialized"/>
		IExtensionProvider Extensions { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store is initialized (the product list is loaded from native store); <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="IsBusy"/>
		bool IsInitialized { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store has pending purchase operation; <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="IsInitialized"/>
		bool IsBusy { get; }

		/// <summary>
		/// Initializes the store. Does nothing (returns a completed operation) if already initialized.
		/// </summary>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Fetch"/>
		/// <seealso cref="Purchase(string)"/>
		IStoreOperation Initialize();

#if !NET35
		/// <summary>
		/// Initializes the store. Does nothing (returns a completed task) if already initialized.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Initialize"/>
		/// <seealso cref="FetchAsync"/>
		/// <seealso cref="PurchaseAsync(string)"/>
		Task InitializeAsync();
#endif

		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Initialize"/>
		/// <seealso cref="Purchase(string)"/>
		IStoreOperation Fetch();

#if !NET35
		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <exception cref="StoreFetchException">Thrown if fetching fails.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Fetch"/>
		/// <seealso cref="InitializeAsync"/>
		/// <seealso cref="PurchaseAsync(string)"/>
		Task FetchAsync();
#endif

		/// <summary>
		/// Initiates purchasing the specified product.
		/// </summary>
		/// <remarks>
		/// If <see cref="Initialize"/> or <see cref="Fetch"/> is in progress, waits for them to complete before proceed.
		/// If the store is not initialized yet calls <see cref="Initialize"/> first. Please note that the call would fail
		/// if another purchase operation is pending. Use <see cref="IsBusy"/> to determine if that is the case.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example another purchase operation is pending).</exception>
		/// <exception cref="StorePurchaseException">Thrown in case of purchase-related errors.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Initialize"/>
		/// <seealso cref="Fetch"/>
		/// <seealso cref="IsBusy"/>
		IStoreOperation<PurchaseResult> Purchase(string productId);

#if !NET35
		/// <summary>
		/// Initiates purchasing the specified product.
		/// </summary>
		/// <remarks>
		/// If <see cref="InitializeAsync"/> or <see cref="FetchAsync"/> is in progress, waits for them to complete before proceed.
		/// If the store is not initialized yet calls <see cref="InitializeAsync"/> first. Please note that the call would fail
		/// if another purchase operation is pending. Use <see cref="IsBusy"/> to determine if that is the case.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example another purchase operation is pending).</exception>
		/// <exception cref="StoreFetchException">Thrown if the store initialization/fetch triggered/awaited by the call fails.</exception>
		/// <exception cref="StorePurchaseException">Thrown in case of purchase-related errors.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Purchase(string)"/>
		/// <seealso cref="InitializeAsync"/>
		/// <seealso cref="FetchAsync"/>
		/// <seealso cref="IsBusy"/>
		Task<PurchaseResult> PurchaseAsync(string productId);
#endif
	}
}
