// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
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
		/// The manager was disabled while a purchase operation was pending.
		/// </summary>
		StoreDisabled,

		/// <summary>
		/// The store initializatino failed.
		/// </summary>
		StoreInitializationFailed,

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
	/// A generic platform store.
	/// </summary>
	public interface IPlatformStore
	{
		/// <summary>
		/// Triggered when a new purchase is initiated.
		/// </summary>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Triggered when a purchase has completed successfully.
		/// </summary>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		/// <summary>
		/// Triggered when a purchase has failed.
		/// </summary>
		event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

		/// <summary>
		/// Returns a collection of store items. Read only.
		/// </summary>
		IStoreProductCollection Products { get; }

		/// <summary>
		/// Returns Unity3d store controller. Read only.
		/// </summary>
		IStoreController Controller { get; }

		/// <summary>
		/// Returns <c>true</c> if the manager is initialized (the product list is loaded from native store); <c>false</c> otherwise. Read only.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Initiates purchasing a product.
		/// </summary>
		/// <param name="productId">Product identifier as specified in the store.</param>
		/// <exception cref="StorePurchaseException">Throw if an purchase-related errors.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the method is called on a disposed store instance.</exception>
		Task<Product> PurchaseAsync(string productId);
	}
}
