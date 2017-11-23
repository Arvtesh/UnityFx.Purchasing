// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A read-only collection of <see cref="IStoreService"/> products.
	/// </summary>
	public interface IStoreProductCollection : IReadOnlyCollection<Product>
	{
		/// <summary>
		/// Returns product for the specified identifier. Read only.
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		Product this[string productId] { get; }

		/// <summary>
		/// Gets the product that is associated with the specified identifier.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		/// <param name="product">The product instance.</param>
		bool TryGetValue(string productId, out Product product);

		/// <summary>
		/// Determines whether a product with the specified identifier is present in the collection.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		bool ContainsKey(string productId);
	}
}
