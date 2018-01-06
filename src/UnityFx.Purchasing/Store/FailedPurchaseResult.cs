﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result for failed purchases.
	/// </summary>
	public class FailedPurchaseResult : PurchaseResult
	{
		/// <summary>
		/// Returns the product identifier. Read only.
		/// </summary>
		public string ProductId { get; }

		/// <summary>
		/// Returns an error that caused the purchase to fail. Read only.
		/// </summary>
		public StorePurchaseError Error { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase operation has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsCanceled => Error == StorePurchaseError.UserCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="FailedPurchaseResult"/> class.
		/// </summary>
		public FailedPurchaseResult(string productId, PurchaseResult purchaseResult, StorePurchaseError error, Exception e)
			: base(purchaseResult.TransactionInfo, purchaseResult.ValidationResult)
		{
			ProductId = productId;
			Error = error;
			Exception = e;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FailedPurchaseResult"/> class.
		/// </summary>
		public FailedPurchaseResult(string productId, StoreTransaction transactionInfo, PurchaseValidationResult validationResult, StorePurchaseError error, Exception e)
			: base(transactionInfo, validationResult)
		{
			ProductId = productId;
			Error = error;
			Exception = e;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="FailedPurchaseResult"/> class.
		/// </summary>
		public FailedPurchaseResult(string productId, StorePurchaseException e)
			: base(e.Result.TransactionInfo, e.Result.ValidationResult)
		{
			ProductId = productId;
			Error = e.Reason;
			Exception = e;
		}
	}
}
