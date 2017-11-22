// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Default implementation of <see cref="IStoreProduct"/>.
	/// </summary>
	public class StoreProduct : IStoreProduct
	{
		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreProduct"/> class.
		/// </summary>
		public StoreProduct(ProductDefinition productDefinition)
		{
			Definition = productDefinition;
		}

		#endregion

		#region IStoreProduct

		/// <inheritdoc/>
		public string Id => Definition.id;

		/// <inheritdoc/>
		public ProductDefinition Definition { get; }

		/// <inheritdoc/>
		public ProductMetadata Metadata { get; set; }

		#endregion
	}
}
