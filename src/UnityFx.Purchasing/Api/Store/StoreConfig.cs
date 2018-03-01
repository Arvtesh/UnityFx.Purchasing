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
	public class StoreConfig
	{
		/// <summary>
		/// Gets the validation status.
		/// </summary>
		public IEnumerable<ProductDefinition> Products { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreConfig"/> class.
		/// </summary>
		public StoreConfig(IEnumerable<ProductDefinition> products)
		{
			Products = products;
		}
	}
}
