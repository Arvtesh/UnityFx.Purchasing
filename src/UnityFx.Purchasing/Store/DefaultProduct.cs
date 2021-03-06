﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Default implementation of <see cref="IStoreProduct"/>.
	/// </summary>
	internal class DefaultProduct : IStoreProduct
	{
		#region interface

		public DefaultProduct(ProductDefinition productDefinition)
		{
			Definition = productDefinition;
		}

		#endregion

		#region IStoreProduct

		public string Id => Definition.id;
		public ProductDefinition Definition { get; }
		public ProductMetadata Metadata { get; set; }

		#endregion
	}
}
