// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store transaction information.
	/// </summary>
	/// <seealso cref="IPlatformStore"/>
	public class StoreTransaction
	{
		/// <summary>
		/// Returns the product selected for purchase. Read only.
		/// </summary>
		public IStoreProduct Product { get; }

		/// <summary>
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		public string TransactionId { get; internal set; }

		/// <summary>
		/// Returns identifier of the target store. Read only.
		/// </summary>
		public string StoreId { get; internal set; }

		/// <summary>
		/// Returns native transaction receipt (differs from Unity receipt). Read only.
		/// </summary>
		public string Receipt { get; internal set; }

		/// <summary>
		/// Returns <c>true</c> if the purchase was auto-restored; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsRestored { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(IStoreProduct product)
		{
			Product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(IStoreProduct product, string transactionId, string storeId, string receipt, bool isRestored)
		{
			Product = product;
			StoreId = storeId;
			TransactionId = transactionId;
			Receipt = receipt;
			IsRestored = isRestored;
		}
	}
}
