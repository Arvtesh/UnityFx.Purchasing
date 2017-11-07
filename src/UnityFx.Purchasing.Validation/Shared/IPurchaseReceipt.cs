// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// A generic purchase receipt.
	/// </summary>
	public interface IPurchaseReceipt
	{
		/// <summary>
		/// Returns the transaction identifier of the item that was purchased. Read only.
		/// </summary>
		string TransactionId { get; }

		/// <summary>
		/// Returns the product identifier of the item that was purchased (SKU). Read only.
		/// </summary>
		string ProductId { get; }

		/// <summary>
		/// Returns the number of items purchased (<c>0</c> if not applicable). Read only.
		/// </summary>
		int Quantity { get; }

		/// <summary>
		/// Returns the date of the purchase (renew) operation. Read only.
		/// </summary>
		DateTime Timestamp { get; }

		/// <summary>
		/// Returns the date of the purchase (or the date of the first subscription) operation. Read only.
		/// </summary>
		DateTime Timestamp0 { get; }
	}
}
