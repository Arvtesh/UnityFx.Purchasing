// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	partial class PurchaseService : IStoreProductCollection
	{
		#region data

		private const string _errorKeyNotFound = "Product '{0}' not found.";

		#endregion

		#region IStoreProductCollection
		#endregion

		#region IReadOnlyCollection

		public Product this[string productId]
		{
			get
			{
				if (productId == null)
				{
					throw new ArgumentNullException(nameof(productId));
				}

				if (_storeController != null)
				{
					var result = _storeController.products.WithID(productId);

					if (result != null)
					{
						return result;
					}
				}

				throw new KeyNotFoundException(string.Format(_errorKeyNotFound, productId));
			}
		}

		public int Count
		{
			get
			{
				if (_storeController != null)
				{
					return _storeController.products.all.Length;
				}

				return 0;
			}
		}

		public bool ContainsKey(string productId)
		{
			if (_storeController != null && productId != null)
			{
				return _storeController.products.WithID(productId) != null;
			}

			return false;
		}

		public bool TryGetValue(string productId, out Product product)
		{
			if (_storeController != null && productId != null)
			{
				product = _storeController.products.WithID(productId);
				return product != null;
			}

			product = null;
			return false;
		}

		#endregion

		#region IEnumerable

		public IEnumerator<Product> GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		#endregion

		#region implementation

		private IEnumerator<Product> GetEnumeratorInternal()
		{
			if (_storeController != null)
			{
				foreach (var product in _storeController.products.all)
				{
					yield return product;
				}
			}
		}

		#endregion
	}
}
