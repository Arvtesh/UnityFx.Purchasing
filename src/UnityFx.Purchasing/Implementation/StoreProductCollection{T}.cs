// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	internal class StoreProductCollection<T> : IStoreProductCollection<T>
	{
		#region data

		private readonly Dictionary<string, T> _products = new Dictionary<string, T>();

		#endregion

		#region interface

		public void Add(string productId, T product) => _products.Add(productId, product);
		public bool Remove(string productId) => _products.Remove(productId);
		public void Clear() => _products.Clear();

		#endregion

		#region IStoreProductCollection

		public T this[string productId] => _products[productId];
		public bool TryGetProduct(string productId, out T product) => _products.TryGetValue(productId, out product);
		public bool Contains(string productId) => _products.ContainsKey(productId);

		#endregion

		#region IReadOnlyCollection

		public int Count => _products.Count;

		#endregion

		#region IEnumerable

		public IEnumerator<T> GetEnumerator() => _products.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _products.Values.GetEnumerator();

		#endregion

		#region implementation
		#endregion
	}
}
