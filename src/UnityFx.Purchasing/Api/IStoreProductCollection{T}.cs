// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A read-only collection of <see cref="IStoreService{TProduct}"/> products.
	/// </summary>
	public interface IStoreProductCollection<TProduct> : IReadOnlyCollection<TProduct> where TProduct : IStoreProduct
	{
		/// <summary>
		/// Returns product for the specified identifier. Read only.
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		TProduct this[string productId] { get; set; }

		/// <summary>
		/// 
		/// </summary>
		bool TryGetValue(string productId, out TProduct product);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		bool ContainsKey(string productId);
	}
}
