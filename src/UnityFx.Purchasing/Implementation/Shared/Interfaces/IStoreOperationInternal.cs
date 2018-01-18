// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// tt
	/// </summary>
	internal interface IStoreOperationInternal : IStoreOperation
	{
		/// <summary>
		/// tt
		/// </summary>
		StoreOperationId Type { get; }

		/// <summary>
		/// tt
		/// </summary>
		object Owner { get; }
	}
}
