// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Purchase event arguments.
	/// </summary>
	public class PurchaseEventArgs : StoreOperationEventArgs
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
		/// Initializes a new instance of the <see cref="PurchaseEventArgs"/> class.
		/// </summary>
		public PurchaseEventArgs(IStoreOperation op, string productId, bool restored)
			: base(op)
		{
			ProductId = productId;
			IsRestored = restored;
		}
	}
}
