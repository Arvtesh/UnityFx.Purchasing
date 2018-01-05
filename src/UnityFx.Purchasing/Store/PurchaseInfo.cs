// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result.
	/// </summary>
	public class PurchaseInfo : PurchaseResult
	{
		/// <summary>
		/// Returns the product identifier. Read only.
		/// </summary>
		public string ProductId { get; }

		/// <summary>
		/// Returns the purchase error (if any). Read only.
		/// </summary>
		public StorePurchaseError? Error { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase operation has completed successfully; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsSucceeded => !Error.HasValue;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase operation has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsFailed => Error.HasValue;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase operation has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsCanceled => Error == StorePurchaseError.UserCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInfo"/> class.
		/// </summary>
		public PurchaseInfo(string productId, PurchaseResult purchaseResult, StorePurchaseError? error, Exception e)
			: base(purchaseResult.TransactionInfo, purchaseResult.ValidationResult)
		{
			ProductId = productId;
			Error = error;
			Exception = e;
		}
	}
}
