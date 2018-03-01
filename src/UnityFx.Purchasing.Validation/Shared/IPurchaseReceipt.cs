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
		/// Gets the transaction identifier of the item that was purchased.
		/// </summary>
		string TransactionId { get; }

		/// <summary>
		/// Gets the product identifier of the item that was purchased (SKU).
		/// </summary>
		string ProductId { get; }

		/// <summary>
		/// Gets the number of items purchased (<c>0</c> if not applicable).
		/// </summary>
		int Quantity { get; }

		/// <summary>
		/// Gets the date of the purchase (renew) operation.
		/// </summary>
		DateTime Timestamp { get; }

		/// <summary>
		/// Gets the date of the purchase (or the date of the first subscription) operation.
		/// </summary>
		DateTime Timestamp0 { get; }
	}
}
