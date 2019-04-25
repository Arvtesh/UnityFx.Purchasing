﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A read-only collection of <see cref="IStoreService"/> products.
	/// </summary>
	/// <typeparam name="T">Product type.</typeparam>
	/// <seealso cref="IStoreService"/>
	public interface IStoreProductCollection<T> : IReadOnlyCollection<T>
	{
		/// <summary>
		/// Gets product for the specified identifier.
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if no product with the specified identifier found.</exception>
		T this[string productId] { get; }

		/// <summary>
		/// Gets a product that is associated with the specified identifier.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		/// <param name="product">The product instance.</param>
		/// <returns>Operation success flag.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		bool TryGetProduct(string productId, out T product);

		/// <summary>
		/// Determines whether a product with the specified identifier is present in the collection.
		/// </summary>
		/// <param name="productId">The product identifier.</param>
		/// <returns>A value that indicates if the product exists in the collection.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="productId"/> is <see langword="null"/>.</exception>
		bool Contains(string productId);
	}
}