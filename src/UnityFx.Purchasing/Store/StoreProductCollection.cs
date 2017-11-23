// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implementation of <see cref="IStoreProductCollection"/>.
	/// </summary>
	internal class StoreProductCollection : IStoreProductCollection
	{
		#region data

		private IStoreController _storeController;

		#endregion

		#region interface

		public void Initialize(IStoreController storeController) => _storeController = storeController;

		#endregion

		#region IStoreProductCollection

		public Product this[string productId] => _storeController?.products.WithID(productId);

		public bool TryGetValue(string productId, out Product product)
		{
			if (_storeController != null)
			{
				product = _storeController.products.WithID(productId);
			}
			else
			{
				product = null;
			}

			return product != null;
		}

		public bool ContainsKey(string productId)
		{
			return _storeController != null && _storeController.products.WithID(productId) != null;
		}

		#endregion

		#region IReadOnlyCollection

		public int Count => _storeController.products.set.Count;

		#endregion

		#region IEnumerable

		public IEnumerator<Product> GetEnumerator() => _storeController.products.set.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _storeController.products.set.GetEnumerator();

		#endregion
	}
}
