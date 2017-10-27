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
		public PurchaseResult Result { get; }

		/// <summary>
		/// Returns an error that caused the purchase to fail. Read only.
		/// </summary>
		public StorePurchaseError Error { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseFailedEventArgs"/> class.
		/// </summary>
		public PurchaseFailedEventArgs(PurchaseResult result, StorePurchaseError error, Exception e)
		{
			Result = result;
			Error = error;
			Exception = e;
		}
	}
}
