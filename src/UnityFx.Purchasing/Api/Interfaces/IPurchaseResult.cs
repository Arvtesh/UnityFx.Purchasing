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
		/// Returns the purchased product or <see langword="null"/> if not available. Read only.
		/// </summary>
		Product Product { get; }

		/// <summary>
		/// Returns identifier of the transaction or <see langword="null"/> if not available. Read only.
		/// </summary>
		string TransactionId { get; }

		/// <summary>
		/// Returns platform transaction receipt (differs from Unity receipt) or <see langword="null"/> if not available. Read only.
		/// </summary>
		string Receipt { get; }

		/// <summary>
		/// Returns product validation result or <see langword="null"/> if not available. Read only.
		/// </summary>
		PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		bool Restored { get; }
	}
}
