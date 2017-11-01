// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	public class AppStoreInAppReceipt
	{
		/// <summary>
		/// The number of items purchased.
		/// </summary>
		public int Quantity { get; internal set; }

		/// <summary>
		/// The product identifier of the item that was purchased.
		/// </summary>
		public string ProductId { get; internal set; }

		/// <summary>
		/// A string that the App Store uses to uniquely identify the application that created the transaction.
		/// </summary>
		public string AppItemId { get; internal set; }

		/// <summary>
		/// The transaction identifier of the item that was purchased.
		/// </summary>
		public string TransactionId { get; internal set; }

		/// <summary>
		/// For a transaction that restores a previous transaction, the transaction identifier of the original transaction. Otherwise, identical to the transaction identifier.
		/// </summary>
		public string OriginalTransactionId { get; internal set; }
	}
}
