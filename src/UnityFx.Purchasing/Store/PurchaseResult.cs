// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result.
	/// </summary>
	public class PurchaseResult
	{
		/// <summary>
		/// Returns the purchased product (or <c>null</c>). Read only.
		/// </summary>
		public IStoreProduct Product { get; }

		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		public StoreTransaction TransactionInfo { get; }

		/// <summary>
		/// Returns product validation result (<c>null</c> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(IStoreProduct product)
		{
			Product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(IStoreProduct product, StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			Product = product;
			TransactionInfo = transactionInfo;
			ValidationResult = validationResult;
		}
	}
}
