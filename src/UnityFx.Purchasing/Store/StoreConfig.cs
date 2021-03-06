﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store configuration.
	/// </summary>
	/// <seealso cref="IStoreDelegate"/>
	public class StoreConfig
	{
		/// <summary>
		/// Returns the validation status. Read only.
		/// </summary>
		public IEnumerable<IStoreProduct> Products { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreConfig"/> class.
		/// </summary>
		public StoreConfig(IEnumerable<IStoreProduct> products)
		{
			Products = products;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreConfig"/> class.
		/// </summary>
		public StoreConfig(IEnumerable<ProductDefinition> products)
		{
			var items = new List<IStoreProduct>();

			foreach (var productDefinitions in products)
			{
				items.Add(new DefaultProduct(productDefinitions));
			}

			Products = items;
		}
	}
}
