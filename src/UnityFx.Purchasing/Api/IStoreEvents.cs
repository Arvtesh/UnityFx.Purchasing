// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Defines store-related events/notifications.
	/// </summary>
	public interface IStoreEvents
	{
		/// <summary>
		/// Triggered when the store initialization has been initiated.
		/// </summary>
		event EventHandler<StoreFetchEventArgs> StoreFetchInitiated;

		/// <summary>
		/// Triggered when the store has been initialized.
		/// </summary>
		event EventHandler<StoreFetchEventArgs> StoreFetchCompleted;

		/// <summary>
		/// Triggered when the store initialization has failed.
		/// </summary>
		event EventHandler<StoreFetchFailedEventArgs> StoreFetchFailed;

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

#if !NET35

		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		IObservable<PurchaseResult> Purchases { get; }

		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		IObservable<FailedPurchaseResult> FailedPurchases { get; }

#endif
	}
}
