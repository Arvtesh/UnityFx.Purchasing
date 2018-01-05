// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result.
	/// </summary>
	public class PurchaseResult
	{
		/// <summary>
		/// Returns the purchased product (or <see langword="null"/>). Read only.
		/// </summary>
		public Product Product { get; }

		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		public StoreTransaction TransactionInfo { get; }

		/// <summary>
		/// Returns product validation result (<see langword="null"/> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(Product product)
		{
			Product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			Product = transactionInfo.Product;
			TransactionInfo = transactionInfo;
			ValidationResult = validationResult;
		}
	}
}
