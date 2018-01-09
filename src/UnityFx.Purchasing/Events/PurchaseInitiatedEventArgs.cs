// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseInitiated"/>.
	/// </summary>
	public class PurchaseInitiatedEventArgs : EventArgs
	{
		/// <summary>
		/// Returns the item selected for purchase. Read only.
		/// </summary>
		public string ProductId { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsRestored { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInitiatedEventArgs"/> class.
		/// </summary>
		public PurchaseInitiatedEventArgs(string productId, bool restored)
		{
			ProductId = productId;
			IsRestored = restored;
		}
	}
}
