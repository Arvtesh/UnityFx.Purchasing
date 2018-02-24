// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif
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
		/// <seealso cref="PurchaseAsync(string)"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="PurchaseCompleted"/>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Raised when a purchase has completed successfully.
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="PurchaseAsync(string)"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

#if UNITYFX_SUPPORT_OBSERVABLES

		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <value>Observable that can be used to track successful purchases.</value>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="FailedPurchases"/>
		IObservable<PurchaseResult> Purchases { get; }

		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <value>Observable that can be used to track failed purchases.</value>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="Purchases"/>
		IObservable<FailedPurchaseResult> FailedPurchases { get; }

#endif

		/// <summary>
		/// Returns store products list. Read only.
		/// </summary>
		/// <value>Read-only collection of products available in the store.</value>
		IStoreProductCollection Products { get; }

		/// <summary>
		/// Returns <c>Unity3d</c> store controller. Returns <see langword="null"/> if the store is not initialized. Read only.
		/// </summary>
		/// <value><c>Unity3d</c> controller that is responsible for all store operations.</value>
		/// <seealso cref="IsInitialized"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		IStoreController Controller { get; }

		/// <summary>
		/// Returns store extensions provider. Returns <see langword="null"/> if the store is not initialized. Read only.
		/// </summary>
		/// <value><c>Unity3d</c> controller that provides access for store-specific extensions.</value>
		/// <seealso cref="IsInitialized"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		IExtensionProvider Extensions { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store is initialized (the product list has been loaded from platform store); <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <value>A value indicating whether the store has been initialized.</value>
		/// <seealso cref="IsBusy"/>
		bool IsInitialized { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the store has pending operations; <see langword="false"/> otherwise. Read only.
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
		/// <exception cref="InvalidOperationException">Thrown if the store is already initialized.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="PurchaseAsync(string)"/>
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
		/// <seealso cref="PurchaseAsync(string)"/>
		IAsyncOperation FetchAsync();

		/// <summary>
		/// Initiates purchase of the specified product.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="PurchaseAsync(string, object)"/> documentation for more information.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
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
		IAsyncOperation<PurchaseResult> PurchaseAsync(string productId);

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
		/// <seealso cref="PurchaseAsync(string)"/>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="IsBusy"/>
		IAsyncOperation<PurchaseResult> PurchaseAsync(string productId, object stateObject);

#if UNITYFX_SUPPORT_APM

		/// <summary>
		/// Begins an asynchronous initialize operation.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="InitializeAsync()"/> documentation for more information.
		/// </remarks>
		/// <param name="userCallback">The method to be called when the asynchronous initialize operation is completed.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous initialize operation from other operations.</param>
		/// <returns>An object that references the asynchronous initialize operation.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the store is already initialized.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="EndInitialize(IAsyncResult)"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		IAsyncResult BeginInitialize(AsyncCallback userCallback, object stateObject);

		/// <summary>
		/// Waits for the pending asynchronous initialize operation to complete.
		/// </summary>
		/// <remarks>
		/// The method will block until the operation has completed. <see cref="EndInitialize(IAsyncResult)"/> must be called
		/// exactly for every call to <see cref="BeginInitialize(AsyncCallback, object)"/>.
		/// </remarks>
		/// <param name="asyncResult">The reference to the pending asynchronous operation to wait for.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncResult"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="asyncResult"/> object was not created by calling <see cref="BeginInitialize(AsyncCallback, object)"/> on this class.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="EndInitialize(IAsyncResult)"/> is called multiple times.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if either the store or <paramref name="asyncResult"/> is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="BeginInitialize(AsyncCallback, object)"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void EndInitialize(IAsyncResult asyncResult);

		/// <summary>
		/// Begins an asynchronous fetch operation.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="FetchAsync()"/> documentation for more information.
		/// </remarks>
		/// <param name="userCallback">The method to be called when the asynchronous fetch operation is completed.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous fetch operation from other operations.</param>
		/// <returns>An object that references the asynchronous fetch operation.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the store is not initialized.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="EndFetch(IAsyncResult)"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		IAsyncResult BeginFetch(AsyncCallback userCallback, object stateObject);

		/// <summary>
		/// Waits for the pending asynchronous fetch operation to complete.
		/// </summary>
		/// <remarks>
		/// The method will block until the operation has completed. <see cref="EndFetch(IAsyncResult)"/> must be called
		/// exactly for every call to <see cref="BeginFetch(AsyncCallback, object)"/>.
		/// </remarks>
		/// <param name="asyncResult">The reference to the pending asynchronous operation to wait for.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncResult"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="asyncResult"/> object was not created by calling <see cref="BeginFetch(AsyncCallback, object)"/> on this class.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if either the store or <paramref name="asyncResult"/> is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="BeginFetch(AsyncCallback, object)"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void EndFetch(IAsyncResult asyncResult);

		/// <summary>
		/// Begins an asynchronous purchase operation of the specified product.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="PurchaseAsync(string, object)"/> documentation for more information.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <param name="userCallback">The method to be called when the asynchronous purchase operation is completed.</param>
		/// <param name="stateObject">A user-provided object that distinguishes this particular asynchronous purchase operation from other operations.</param>
		/// <returns>An object that references the asynchronous purchase operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example too many concurrent purchase operations).</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="EndPurchase(IAsyncResult)"/>
		/// <seealso cref="IsBusy"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		IAsyncResult BeginPurchase(string productId, AsyncCallback userCallback, object stateObject);

		/// <summary>
		/// Waits for the pending asynchronous purchase operation to complete.
		/// </summary>
		/// <remarks>
		/// The method will block until the operation has completed. <see cref="EndPurchase(IAsyncResult)"/> must be called
		/// exactly for every call to <see cref="BeginPurchase(string, AsyncCallback, object)"/>.
		/// </remarks>
		/// <param name="asyncResult">The reference to the pending asynchronous operation to wait for.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="asyncResult"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="asyncResult"/> object was not created by calling <see cref="BeginPurchase(string, AsyncCallback, object)"/> on this class.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="EndPurchase(IAsyncResult)"/> is called multiple times.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if either the store or <paramref name="asyncResult"/> is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/asynchronous-programming-model-apm">Asynchronous Programming Model (APM)</seealso>
		/// <seealso cref="BeginPurchase(string, AsyncCallback, object)"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		PurchaseResult EndPurchase(IAsyncResult asyncResult);

#endif

#if UNITYFX_SUPPORT_TAP

		/// <summary>
		/// Initiates the store initialization.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="InitializeAsync()"/> documentation for more information.
		/// </remarks>
		/// <returns>A <see cref="Task"/> instance that can be used to track the operation progress.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the store is already initialized.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if initialization fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap">Task-based Asynchronous Pattern (TAP)</seealso>
		/// <seealso cref="FetchTaskAsync()"/>
		/// <seealso cref="PurchaseTaskAsync(string)"/>
		Task InitializeTaskAsync();

		/// <summary>
		/// Fetches product information from the store.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="FetchAsync()"/> documentation for more information.
		/// </remarks>
		/// <returns>A <see cref="Task"/> instance that can be used to track the operation progress.</returns>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store is not initialized.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if fetching fails.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap">Task-based Asynchronous Pattern (TAP)</seealso>
		/// <seealso cref="InitializeTaskAsync()"/>
		/// <seealso cref="PurchaseTaskAsync(string)"/>
		Task FetchTaskAsync();

		/// <summary>
		/// Initiates purchase of the specified product.
		/// </summary>
		/// <remarks>
		/// Please see <see cref="PurchaseAsync(string, object)"/> documentation for more information.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <returns>A <see cref="Task"/> instance that can be used to track the operation progress.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="productId"/> is an empty string.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the store state does not allow purchases (for example too many concurrent purchase operations).</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <exception cref="StoreFetchException">Thrown if the store initialization/fetch triggered/awaited by the call fails.</exception>
		/// <exception cref="StorePurchaseException">Thrown in case of purchase-related errors.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap">Task-based Asynchronous Pattern (TAP)</seealso>
		/// <seealso cref="InitializeTaskAsync()"/>
		/// <seealso cref="FetchTaskAsync()"/>
		/// <seealso cref="IsBusy"/>
		Task<PurchaseResult> PurchaseTaskAsync(string productId);

#endif
	}
}
