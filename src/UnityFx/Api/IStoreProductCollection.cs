// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A collection of <see cref="Product"/> instances.
	/// </summary>
	public interface IStoreProductCollection : IReadOnlyCollection<Product>
	{
	}
}
