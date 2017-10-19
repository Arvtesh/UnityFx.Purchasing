// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	partial class StoreService : IStoreProductCollection
	{
		#region data
		#endregion

		#region IStoreProductCollection
		#endregion

		#region IReadOnlyCollection

		public IStoreProduct this[string productId]
		{
			get
			{
				ThrowIfInvalidProductId(productId);
				return _products[productId];
			}
		}

		public int Count
		{
			get
			{
				return _products.Count;
			}
		}

		public bool ContainsKey(string productId)
		{
			ThrowIfInvalidProductId(productId);
			return _products.ContainsKey(productId);
		}

		public bool TryGetValue(string productId, out IStoreProduct product)
		{
			ThrowIfInvalidProductId(productId);
			return _products.TryGetValue(productId, out product);
		}

		#endregion

		#region IEnumerable

		public IEnumerator<IStoreProduct> GetEnumerator()
		{
			return _products.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _products.Values.GetEnumerator();
		}

		#endregion
	}
}
