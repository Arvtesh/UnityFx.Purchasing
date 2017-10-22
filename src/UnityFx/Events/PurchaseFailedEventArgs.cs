// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event argument for <see cref="IPlatformStore.PurchaseFailed"/>.
	/// </summary>
	public class PurchaseFailedEventArgs : EventArgs
	{
		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		public StoreTransaction TransactionInfo { get; }

		/// <summary>
		/// Returns product validation result (<c>null</c> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Returns an error that caused the purchase to fail. Read only.
		/// </summary>
		public StorePurchaseError Error { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseFailedEventArgs"/> class.
		/// </summary>
		public PurchaseFailedEventArgs(StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError error)
		{
			TransactionInfo = transactionInfo;
			ValidationResult = validationResult;
			Error = error;
		}
	}
}
