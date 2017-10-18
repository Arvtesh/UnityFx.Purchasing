// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store manager.
	/// </summary>
	public interface IStoreService : IPlatformStore, IDisposable
	{
		/// <summary>
		/// Initializes the manager. The method should only be called once.
		/// </summary>
		/// <exception cref="StoreInitializeException">Thrown if store initializatino fails.</exception>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="items"/> is <c>null</c>.</exception>
		Task InitializeAsync(IEnumerable<ProductDefinition> items);
	}
}
