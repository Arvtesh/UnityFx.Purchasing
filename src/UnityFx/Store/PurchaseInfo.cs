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
		/// Returns the purchase error (if any). Read only.
		/// </summary>
		public StorePurchaseError? Error { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns <c>true</c> if the purchase operation has completed successfully; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsSucceeded => !Error.HasValue;

		/// <summary>
		/// Returns <c>true</c> if the purchase operation has failed; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsFailed => Error.HasValue;

		/// <summary>
		/// Returns <c>true</c> if the purchase operation has failed; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsCanceled => Error == StorePurchaseError.UserCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInfo"/> class.
		/// </summary>
		public PurchaseInfo(IStoreProduct product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError? error, Exception e)
			: base(product, transactionInfo, validationResult)
		{
			Error = error;
			Exception = e;
		}
	}
}
