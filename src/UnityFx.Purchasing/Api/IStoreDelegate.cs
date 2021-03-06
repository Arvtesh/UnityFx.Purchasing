﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A delegate object for <see cref="IStoreService"/>.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	public interface IStoreDelegate
	{
		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task{TResult}"/> with <c>null</c> result value to indicate that no validation is needed.
		/// </summary>
		/// <param name="product">Reference to a product being purchased.</param>
		/// <param name="transactionInfo">The transaction data to validate.</param>
		Task<PurchaseValidationResult> ValidatePurchaseAsync(IStoreProduct product, StoreTransaction transactionInfo);
	}
}
