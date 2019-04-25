// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Events for the <see cref="IStoreService"/>.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	public interface IStoreEvents
	{
		/// <summary>
		/// Raised when the store INITIALIZE operation has been initiated.
		/// </summary>
		/// <seealso cref="InitializeCompleted"/>
		event EventHandler<AsyncInitiatedEventArgs> InitializeInitiated;

		/// <summary>
		/// Raised when the store INITIALIZE operation has been completed (either successfully or not).
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="InitializeInitiated"/>
		event EventHandler<FetchCompletedEventArgs> InitializeCompleted;

		/// <summary>
		/// Raised when the store FETCH operation has been initiated.
		/// </summary>
		/// <seealso cref="FetchCompleted"/>
		event EventHandler<AsyncInitiatedEventArgs> FetchInitiated;

		/// <summary>
		/// Raised when the store FETCH operation has been completed (either successfully or not).
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="FetchInitiated"/>
		event EventHandler<FetchCompletedEventArgs> FetchCompleted;

		/// <summary>
		/// Raised when the store RESTORE operation has been initiated.
		/// </summary>
		/// <seealso cref="RestoreCompleted"/>
		event EventHandler<AsyncInitiatedEventArgs> RestoreInitiated;

		/// <summary>
		/// Raised when the store RESTORE operation has been completed (either successfully or not).
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="RestoreInitiated"/>
		event EventHandler<AsyncCompletedEventArgs> RestoreCompleted;

		/// <summary>
		/// Raised when the store PURCHASE operation has been initiated.
		/// </summary>
		/// <seealso cref="PurchaseCompleted"/>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Raised when the store PURCHASE operation has been completed (either successfully or not).
		/// </summary>
		/// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/event-based-asynchronous-pattern-eap">Event-based Asynchronous Pattern (EAP)</seealso>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;
	}
}
