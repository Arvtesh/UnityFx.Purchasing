// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a store purchase.
	/// </summary>
	public interface IPurchaseResult : IStoreOperationInfo
	{
		/// <summary>
		/// Returns the product identifier. Read only.
		/// </summary>
		string ProductId { get; }

		/// <summary>
		/// Returns the purchased product (or <see langword="null"/>). Read only.
		/// </summary>
		Product Product { get; }

		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		StoreTransaction Transaction { get; }

		/// <summary>
		/// Returns product validation result (<see langword="null"/> if not available). Read only.
		/// </summary>
		PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		bool Restored { get; }
	}
}
