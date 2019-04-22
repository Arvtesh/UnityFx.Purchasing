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
	/// <seealso cref="IStoreService"/>
	public interface IStoreConfig
	{
		/// <summary>
		/// Gets the store products.
		/// </summary>
		IEnumerable<ProductDefinition> Products { get; }
	}
}
