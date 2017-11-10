// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreProductCollection{TProduct}"/>.
	/// </summary>
	internal class StoreProductCollection<TProduct> : IStoreProductCollection<TProduct> where TProduct : class, IStoreProduct
	{
		#region data

		private Dictionary<string, TProduct> _products = new Dictionary<string, TProduct>();

		#endregion

		#region interface

		public void Add(string productId, TProduct product) => _products.Add(productId, product);

		public bool Remove(string productId) => _products.Remove(productId);

		public void Clear() => _products.Clear();

		#endregion

		#region IStoreProductCollection

		public TProduct this[string productId] { get => _products[productId]; set => _products[productId] = value; }

		public bool TryGetValue(string productId, out TProduct product) => _products.TryGetValue(productId, out product);

		public bool ContainsKey(string productId) => _products.ContainsKey(productId);

		#endregion

		#region IReadOnlyCollection

		public int Count => _products.Count;

		#endregion

		#region IEnumerable

		public IEnumerator<TProduct> GetEnumerator() => _products.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _products.Values.GetEnumerator();

		#endregion
	}
}
