// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a store purchase.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public interface IPurchaseResult : IStoreTransaction, IStoreOperationInfo
	{
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
