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
		/// <seealso cref="InitializeCompleted"/>
		event EventHandler<FetchInitiatedEventArgs> InitializeInitiated;

		/// <summary>
		/// Triggered when the store has been initialized.
		/// </summary>
		/// <seealso cref="InitializeInitiated"/>
		event EventHandler<FetchCompletedEventArgs> InitializeCompleted;

		/// <summary>
		/// Triggered when the store initialization has been initiated.
		/// </summary>
		/// <seealso cref="FetchCompleted"/>
		event EventHandler<FetchInitiatedEventArgs> FetchInitiated;

		/// <summary>
		/// Triggered when the store has been initialized.
		/// </summary>
		/// <seealso cref="FetchInitiated"/>
		event EventHandler<FetchCompletedEventArgs> FetchCompleted;

		/// <summary>
		/// Triggered when a new purchase is initiated.
		/// </summary>
		/// <seealso cref="PurchaseCompleted"/>
		event EventHandler<PurchaseInitiatedEventArgs> PurchaseInitiated;

		/// <summary>
		/// Triggered when a purchase has completed successfully.
		/// </summary>
		/// <seealso cref="PurchaseInitiated"/>
		event EventHandler<PurchaseCompletedEventArgs> PurchaseCompleted;

#if UNITYFX_SUPPORT_OBSERVABLES

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
