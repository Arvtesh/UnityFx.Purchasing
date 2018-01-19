// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreEvents.PurchaseInitiated"/>.
	/// </summary>
	public class PurchaseInitiatedEventArgs : PurchaseEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInitiatedEventArgs"/> class.
		/// </summary>
		public PurchaseInitiatedEventArgs(IStoreOperation op, string productId, bool restored)
			: base(op, productId, restored)
		{
		}
	}
}
