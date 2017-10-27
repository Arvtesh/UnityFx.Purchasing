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
	public interface IStoreProductCollection : IReadOnlyCollection<IStoreProduct>
	{
		/// <summary>
		/// Returns product for the specified identifier. Read only.
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		IStoreProduct this[string productId] { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productId">The store product identifier.</param>
		bool ContainsKey(string productId);
	}
}
