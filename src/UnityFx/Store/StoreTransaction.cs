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
		/// Returns identifier of the target store. Read only.
		/// </summary>
		public string StoreId { get; }

		/// <summary>
		/// Returns identifier of the transaction (if availbale). Read only.
		/// </summary>
		public string TransactionId { get; }

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
		public bool IsSucceeded { get; }

		/// <summary>
		/// Returns product validation result (<c>null</c> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Returns an error code (if available). Read only.
		/// </summary>
		public StorePurchaseError Error { get; }

		/// <summary>
		/// Returns an exception instance with information in failure (if available). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction()
		{
		}
	}
}
