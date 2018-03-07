// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	internal class StoreProductCollection : IStoreProductCollection<Product>
	{
		#region data

		private IStoreController _storeController;

		#endregion

		#region interface

		public StoreProductCollection(IStoreController storeController)
		{
			_storeController = storeController;
		}

		public void SetController(IStoreController storeController) => _storeController = storeController;

		#endregion

		#region IStoreProductCollection

		public Product this[string productId]
		{
			get
			{
				if (productId == null)
				{
					throw new ArgumentNullException(nameof(productId));
				}

				var result = _storeController?.products.WithID(productId);

				if (result == null)
				{
					throw new KeyNotFoundException("No product found with identifier: " + productId);
				}

				return result;
			}
		}

		public bool TryGetProduct(string productId, out Product product)
		{
			if (productId == null)
			{
				throw new ArgumentNullException(nameof(productId));
			}

			return (product = _storeController?.products.WithID(productId)) != null;
		}

		public bool Contains(string productId)
		{
			if (productId == null)
			{
				throw new ArgumentNullException(nameof(productId));
			}

			return _storeController?.products.WithID(productId) != null;
		}

		#endregion

		#region IReadOnlyCollection

		public int Count => _storeController?.products.set.Count ?? 0;

		#endregion

		#region IEnumerable

		public IEnumerator<Product> GetEnumerator() => GetEnumeratorInternal();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorInternal();

		#endregion

		#region implementation

		private IEnumerator<Product> GetEnumeratorInternal()
		{
			if (_storeController != null)
			{
				return _storeController.products.set.GetEnumerator();
			}

			return Enumerable.Empty<Product>().GetEnumerator();
		}

		#endregion
	}
}
