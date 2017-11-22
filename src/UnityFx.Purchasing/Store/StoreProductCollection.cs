// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreProductCollection"/>.
	/// </summary>
	internal class StoreProductCollection : IStoreProductCollection
	{
		#region data

		private Dictionary<string, IStoreProduct> _products = new Dictionary<string, IStoreProduct>();

		#endregion

		#region interface

		public void Add(string productId, IStoreProduct product) => _products.Add(productId, product);

		public bool Remove(string productId) => _products.Remove(productId);

		public void Clear() => _products.Clear();

		#endregion

		#region IStoreProductCollection

		public IStoreProduct this[string productId] { get => _products[productId]; set => _products[productId] = value; }

		public bool TryGetValue(string productId, out IStoreProduct product) => _products.TryGetValue(productId, out product);

		public bool ContainsKey(string productId) => _products.ContainsKey(productId);

		#endregion

		#region IReadOnlyCollection

		public int Count => _products.Count;

		#endregion

		#region IEnumerable

		public IEnumerator<IStoreProduct> GetEnumerator() => _products.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _products.Values.GetEnumerator();

		#endregion
	}
}
