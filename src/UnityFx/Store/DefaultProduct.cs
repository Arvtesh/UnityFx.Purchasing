// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Default implementation for <see cref="IStoreProduct"/>.
	/// </summary>
	internal class DefaultProduct : IStoreProduct
	{
		public ProductDefinition Definition { get; }
		public ProductMetadata Metadata { get; set; }

		public DefaultProduct(ProductDefinition productDefinition)
		{
			Definition = productDefinition;
		}
	}
}
