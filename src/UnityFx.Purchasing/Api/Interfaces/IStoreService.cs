﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine.Purchasing;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates possible initialization errors.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	public enum StoreFetchError
	{
		/// <summary>
		/// No errors.
		/// </summary>
		None,

		/// <summary>
		/// A catch-all for unrecognized fetch/initialize problems.
		/// </summary>
		Unknown,

		/// <summary>
		/// The manager was disposed while a fetch/initialize operation was pending.
		/// </summary>
		StoreDisposed,

		/// <summary>
		/// The store failed get its configuration.
		/// </summary>
		StoreConfigUnavailable,

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
	/// <seealso cref="IStoreService"/>
	public enum StorePurchaseError
	{
		/// <summary>
		/// No errors.
		/// </summary>
		None,

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
		/// Store validation (either local or not) of purchase receipt failed.
		/// </summary>
		ReceiptValidationFailed,

		/// <summary>
		/// Store validation of purchase receipt not available.
		/// </summary>
		ReceiptValidationNotAvailable
	}

	/// <summary>
	/// An in-app store service based on <c>Unity IAP</c>.
	/// </summary>
	/// <remarks>
	/// TODO
	/// </remarks>
	/// <seealso href="https://docs.unity3d.com/Manual/UnityIAP.html">Unity IAP</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/">Asynchronous Programming Patterns</seealso>
	/// <seealso cref="IStoreController"/>
	/// <seealso cref="IExtensionProvider"/>
	public interface IStoreService
	{
		/// <summary>
		/// Raised when the store initialization has been initiated.
		/// </summary>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="InitializeCompleted"/>
		event EventHandler<FetchInitiatedEventArgs> InitializeInitiated;

		/// <summary>
		/// Raised when the store has been initialized.
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="InitializeInitiated"/>
		event EventHandler<FetchCompletedEventArgs> InitializeCompleted;

		/// <summary>
		/// Raised when the store initialization has been initiated.
		/// </summary>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="FetchCompleted"/>
		event EventHandler<FetchInitiatedEventArgs> FetchInitiated;

		/// <summary>
		/// Raised when the store has been initialized.
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="FetchInitiated"/>
		event EventHandler<FetchCompletedEventArgs> FetchCompleted;

		/// <summary>
		/// Raised when a new purchase is initiated.
		/// </summary>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="PurchaseCompleted"/>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Raised when a purchase has completed successfully.
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

#if !NET35

		/// <summary>
		/// Gets push notification provider of the store transactions.
		/// </summary>
		/// <value>Observable that can be used to track successful purchases.</value>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="FailedPurchases"/>
		IObservable<PurchaseResult> Purchases { get; }

		/// <summary>
		/// Gets push notification provider of the store transactions.
		/// </summary>
		/// <value>Observable that can be used to track failed purchases.</value>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="Purchases"/>
		IObservable<FailedPurchaseResult> FailedPurchases { get; }

#endif

		/// <summary>
		/// Gets a read-only collection of <c>Unity3d</c> products.
		/// </summary>
		/// <value>Read-only collection of <c>Unity3d</c> products available in the store.</value>
		IStoreProductCollection<Product> Products { get; }

		/// <summary>
		/// Gets <c>Unity3d</c> store controller. The value is <see langword="null"/> if the store is not initialized.
		/// </summary>
		/// <value><c>Unity3d</c> controller that is responsible for all store operations.</value>
		/// <seealso cref="IsInitialized"/>
		IStoreController Controller { get; }

		/// <summary>
		/// Gets <c>Unity3d</c> store extensions provider. The value is <see langword="null"/> if the store is not initialized.
		/// </summary>
		/// <value><c>Unity3d</c> controller that provides access for store-specific extensions.</value>
		/// <seealso cref="IsInitialized"/>
		IExtensionProvider Extensions { get; }

		/// <summary>
		/// Gets a value indicating whether the store is initialized (the product list has been loaded from platform store).
		/// </summary>
		/// <value>A value indicating whether the store has been initialized.</value>
		/// <seealso cref="IsBusy"/>
		bool IsInitialized { get; }

		/// <summary>
		/// Gets a value indicating whether the store has pending operations.
		/// </summary>
		/// <value>A value indicating whether the store has pending operations.</value>
		/// <seealso cref="IsInitialized"/>
		bool IsBusy { get; }

		/// <summary>
		/// Initiates the store initialization. Returns a completed operation if store is already initialized.
		/// </summary>
		/// <returns>An object that can be used to track the operation progress.</returns>
		/// <event cref="InitializeInitiated">Raised when the operation is created.</event>
		/// <event cref="InitializeCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		IAsyncOperation InitializeAsync();

		/// <summary>
		/// Initiates fetching product information from the store.
		/// </summary>
		/// <returns>An object that can be used to track the operation progress.</returns>
		/// <event cref="FetchInitiated">Raised when the operation is created.</event>
		/// <event cref="FetchCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="InvalidOperationException">Thrown if the store is not initialized or fetch operation is pending.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		IAsyncOperation FetchAsync();

		/// <summary>
		/// Initiates purchase of the specified product.
		/// </summary>
		/// <remarks>
		/// Internally method does the following:
		/// <list type="number">
		/// <item>If <see cref="InitializeAsync()"/> or <see cref="FetchAsync()"/> operation is pending, waits for its completion.</item>
		/// <item>If another purchase operation is pending, waits for it to complete.</item>
		/// <item>Initiates purchase of the specified product.</item>
		/// <item>If the purchase succeeds initiates its verification.</item>
		/// <item>If all of the former steps succeed, the operation succeeds; otherwise the transaction is treated as failed.</item>
		/// </list>
		/// Please note that the call would fail if another purchase operation is pending. Use <see cref="IsBusy"/> to determine if that is the case.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular operation from others.</param>
		/// <returns>An object that can be used to track the operation progress.</returns>
		/// <event cref="PurchaseInitiated">Raised when the operation is created.</event>
		/// <event cref="PurchaseCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example too many concurrent purchase operations).</exception>
		/// <exception cref="StorePurchaseException">Thrown in case of purchase-related errors.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="IsBusy"/>
		IAsyncOperation<PurchaseResult> PurchaseAsync(string productId, object stateObject = null);
	}
}
