// Copyright (c) Alexander Bogarsukov.
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
		/// Shows a wait popup while purchase operation is in progress. The popup should be dissmissed when the returned handle is disposed.
		/// If not needed method can just return <c>null</c>.
		/// </summary>
		IDisposable BeginWait();

		/// <summary>
		/// Requests the store configuration.
		/// </summary>
		Task<StoreConfig> GetStoreConfigAsync();

		/// <summary>
		/// Validates the purchase. May return a <see cref="Task"/> with <c>null</c> result to indicate that no validation is needed.
		/// </summary>
		/// <param name="product">The product to validate.</param>
		/// <param name="storeId">Store identifier.</param>
		/// <param name="nativeReceipt">Native platform-specific purchase receipt (differs from the one supplied by <see cref="Product"/>).</param>
		Task<PurchaseValidationResult> ValidatePurchaseAsync(Product product, string storeId, string nativeReceipt);
	}
}
