// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
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
		/// Returns identifier of the target store (if availbale). Read only.
		/// </summary>
		public string StoreId { get; }

		/// <summary>
		/// Returns identifier of the transaction (if availbale). Read only.
		/// </summary>
		public string TransactionId { get; }

		/// <summary>
		/// Returns transaction receipt (if availbale). Read only.
		/// </summary>
		public string Receipt { get; }

		/// <summary>
		/// Returns the product selected for purchase. Read only.
		/// </summary>
		public IStoreProduct Product { get; }

		/// <summary>
		/// Returns <c>true</c> if the purchase was auto-restored; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsRestored { get; }

		/// <summary>
		/// Returns <c>true</c> if the purchase was successful; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsSucceeded => !Error.HasValue;

		/// <summary>
		/// Returns an error code (if available). Read only.
		/// </summary>
		public StorePurchaseError? Error { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(IStoreProduct product, string transactionId, string receipt, string storeId, bool isRestored)
		{
			Product = product;
			StoreId = storeId;
			TransactionId = transactionId;
			Receipt = receipt;
			IsRestored = isRestored;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(IStoreProduct product, string transactionId, string receipt, string storeId, bool isRestored, StorePurchaseError error)
		{
			Product = product;
			StoreId = storeId;
			TransactionId = transactionId;
			Receipt = receipt;
			IsRestored = isRestored;
			Error = error;
		}
	}
}
