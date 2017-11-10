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
	public class StoreConfig<TProduct> where TProduct : class, IStoreProduct
	{
		/// <summary>
		/// Returns the validation status. Read only.
		/// </summary>
		public IEnumerable<TProduct> Products { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreConfig{TProduct}"/> class.
		/// </summary>
		public StoreConfig(IEnumerable<TProduct> products)
		{
			Products = products;
		}
	}
}
