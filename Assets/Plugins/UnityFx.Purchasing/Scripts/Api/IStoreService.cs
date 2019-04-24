// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// An in-app store service based on <c>Unity IAP</c>.
	/// </summary>
	/// <seealso href="https://docs.unity3d.com/Manual/UnityIAP.html">Unity IAP</seealso>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/">Asynchronous Programming Patterns</seealso>
	/// <seealso cref="IStoreController"/>
	/// <seealso cref="IExtensionProvider"/>
	public interface IStoreService : IDisposable
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

		/// <summary>
		/// Gets a collection of trace listeners attached to the service.
		/// </summary>
		TraceListenerCollection TraceListeners { get; }

		/// <summary>
		/// Gets trace switch used by the <see cref="TraceSource"/> attached to the service.
		/// </summary>
		SourceSwitch TraceSwitch { get; }

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
		/// Initiates the store initialization. Returns a completed task if store is already initialized.
		/// </summary>
		/// <returns>A <see cref="Task"/> that can be used to track the operation progress.</returns>
		/// <event cref="InitializeInitiated">Raised when the operation is initiated.</event>
		/// <event cref="InitializeCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="InitializeException">Thrown on an initialize-specific error.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="RestoreAsync()"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="IsBusy"/>
		/// <seealso cref="IsInitialized"/>
		Task InitializeAsync();

		/// <summary>
		/// Initiates fetching product information from the store.
		/// </summary>
		/// <returns>A <see cref="Task"/> that can be used to track the operation progress.</returns>
		/// <event cref="FetchInitiated">Raised when the operation is initiated.</event>
		/// <event cref="FetchCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="FetchException">Thrown on a fetch-specific error.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the another async operation is pending.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="RestoreAsync()"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="IsBusy"/>
		Task FetchAsync();

		/// <summary>
		/// Initiates restoring transactions.
		/// </summary>
		/// <returns>A <see cref="Task"/> that can be used to track the operation progress.</returns>
		/// <exception cref="RestoreException">Thrown on a restore-specific error.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the another async operation is pending.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="IsBusy"/>
		Task RestoreAsync();

		/// <summary>
		/// Initiates purchase of the specified product.
		/// </summary>
		/// <remarks>
		/// Please note that the call would fail if another async operation is pending. Use <see cref="IsBusy"/> to determine if that is the case.
		/// </remarks>
		/// <param name="productId">Identifier of a product to purchase as specified in the store.</param>
		/// <param name="asyncState">A user-provided object that distinguishes this particular operation from others.</param>
		/// <returns>An object that can be used to track the operation progress.</returns>
		/// <event cref="PurchaseInitiated">Raised when the operation is initiated.</event>
		/// <event cref="PurchaseCompleted">Raised when the operation has completed (either successfully or not).</event>
		/// <exception cref="PurchaseException">Thrown on a purchase-specific error.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="productId"/> is invalid.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the another async operation is pending.</exception>
		/// <exception cref="PlatformNotSupportedException">Thrown if platform does not support purchasing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the store is disposed.</exception>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="PurchaseAsync(string, object)"/>
		/// <seealso cref="InitializeAsync()"/>
		/// <seealso cref="FetchAsync()"/>
		/// <seealso cref="RestoreAsync()"/>
		/// <seealso cref="IsBusy"/>
		Task<PurchaseResult> PurchaseAsync(string productId, object asyncState = null);
	}
}
