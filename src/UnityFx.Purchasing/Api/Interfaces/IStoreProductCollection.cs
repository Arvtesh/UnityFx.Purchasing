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
#if NET35
	public interface IStoreProductCollection : IEnumerable<Product>
#else
	public interface IStoreProductCollection : IReadOnlyCollection<Product>
#endif
	{
#if NET35
		/// <summary>
		/// Returns the number of elements in the collection. Read only.
		/// </summary>
		int Count { get; }
#endif

		/// <summary>
		/// Returns product for the specified identifier. Read only.
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if no product with the specified identifier found.</exception>
		Product this[string productId] { get; }

		/// <summary>
		/// Gets the product that is associated with the specified identifier.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		/// <param name="product">The product instance.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		bool TryGetProduct(string productId, out Product product);

		/// <summary>
		/// Determines whether a product with the specified identifier is present in the collection.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		bool Contains(string productId);
	}
}
