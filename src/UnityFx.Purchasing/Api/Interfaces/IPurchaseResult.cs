// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a store purchase.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public interface IPurchaseResult : IStoreTransaction
	{
		/// <summary>
		/// Gets product validation result or <see langword="null"/> if not available.
		/// </summary>
		PurchaseValidationResult ValidationResult { get; }
	}
}
