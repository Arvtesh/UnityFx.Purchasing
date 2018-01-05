// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event argument for <see cref="IStoreService.PurchaseFailed"/>.
	/// </summary>
	public class PurchaseFailedEventArgs : EventArgs
	{
		/// <summary>
		/// Returns the purchase result. Read only.
		/// </summary>
		public FailedPurchaseResult Result { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseFailedEventArgs"/> class.
		/// </summary>
		public PurchaseFailedEventArgs(FailedPurchaseResult result)
		{
			Result = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseFailedEventArgs"/> class.
		/// </summary>
		public PurchaseFailedEventArgs(string productId, PurchaseResult result, StorePurchaseError error, Exception e)
		{
			Result = new FailedPurchaseResult(productId, result, error, e);
		}
	}
}
