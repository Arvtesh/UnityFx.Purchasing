// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Defines store-related events/notifications.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	public interface IStoreEvents
	{
		/// <summary>
		/// Triggered when the store initialization has been initiated.
		/// </summary>
		/// <seealso cref="StoreFetchCompleted"/>
		/// <seealso cref="StoreFetchFailed"/>
		event EventHandler<StoreFetchEventArgs> StoreFetchInitiated;

		/// <summary>
		/// Triggered when the store has been initialized.
		/// </summary>
		/// <seealso cref="StoreFetchFailed"/>
		/// <seealso cref="StoreFetchInitiated"/>
		event EventHandler<StoreFetchEventArgs> StoreFetchCompleted;

		/// <summary>
		/// Triggered when the store initialization has failed.
		/// </summary>
		/// <seealso cref="StoreFetchCompleted"/>
		/// <seealso cref="StoreFetchInitiated"/>
		event EventHandler<StoreFetchFailedEventArgs> StoreFetchFailed;

		/// <summary>
		/// Triggered when a new purchase is initiated.
		/// </summary>
		/// <seealso cref="PurchaseCompleted"/>
		/// <seealso cref="PurchaseFailed"/>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Triggered when a purchase has completed successfully.
		/// </summary>
		/// <seealso cref="PurchaseFailed"/>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

		/// <summary>
		/// Triggered when a purchase has failed.
		/// </summary>
		/// <seealso cref="PurchaseCompleted"/>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseFailedEventArgs> PurchaseFailed;

#if !NET35
		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="FailedPurchases"/>
		IObservable<PurchaseResult> Purchases { get; }

		/// <summary>
		/// Returns push notification provider of the store transactions. Read only.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if the store instance is disposed.</exception>
		/// <seealso cref="Purchases"/>
		IObservable<FailedPurchaseResult> FailedPurchases { get; }
#endif
	}
}
