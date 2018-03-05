// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic in-app store service based on <c>Unity IAP</c> for user-defined products.
	/// </summary>
	/// <typeparam name="T">User-defined product type.</typeparam>
	public interface IStoreService<T> : IStoreService
	{
		/// <summary>
		/// Gets a read-only collection of store products.
		/// </summary>
		/// <value>Read-only collection of products available in the store.</value>
		new IStoreProductCollection<T> Products { get; }
	}
}
