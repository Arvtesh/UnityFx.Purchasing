// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store configuration.
	/// </summary>
	public class StoreConfig : IStoreConfig
	{
		#region data

		private readonly List<ProductDefinition> _products = new List<ProductDefinition>();

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreConfig"/> class.
		/// </summary>
		public StoreConfig(IEnumerable<ProductDefinition> products)
		{
			_products.AddRange(products);
		}

		/// <summary>
		/// Adds a product definition to the list.
		/// </summary>
		public void Add(ProductDefinition product)
		{
			_products.Add(product);
		}

		#endregion

		#region interface

		/// <inheritdoc/>
		public IEnumerable<ProductDefinition> Products
		{
			get
			{
				return _products;
			}
		}

		#endregion
	}
}
