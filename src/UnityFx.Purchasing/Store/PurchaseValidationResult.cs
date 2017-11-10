// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates purchase validation results.
	/// </summary>
	public enum PurchaseValidationStatus
	{
		/// <summary>
		/// Validation succeeded.
		/// </summary>
		Ok,

		/// <summary>
		/// Validation failed: the purchase receipt is invalid.
		/// </summary>
		Failure,

		/// <summary>
		/// Result is not available. For example transport error while executing verification request.
		/// </summary>
		NotAvailable,
	}

	/// <summary>
	/// A purchase validation result.
	/// </summary>
	public class PurchaseValidationResult
	{
		/// <summary>
		/// Returns the validation status. Read only.
		/// </summary>
		public PurchaseValidationStatus Status { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(PurchaseValidationStatus status)
		{
			Status = status;
		}
	}
}
