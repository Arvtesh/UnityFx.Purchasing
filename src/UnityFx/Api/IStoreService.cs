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
		/// Initializes the manager (if not initialized already).
		/// </summary>
		/// <exception cref="StoreInitializeException">Thrown if store initialization fails.</exception>
		Task InitializeAsync();
	}
}
