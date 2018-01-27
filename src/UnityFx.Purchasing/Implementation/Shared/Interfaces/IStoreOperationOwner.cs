// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Store operation owner.
	/// </summary>
	internal interface IStoreOperationOwner
	{
		/// <summary>
		/// Returns the parent store.
		/// </summary>
		StoreService Store { get; }

		/// <summary>
		/// Returns the console instance.
		/// </summary>
		TraceSource TraceSource { get; }

		/// <summary>
		/// Registers the specified operation.
		/// </summary>
		void AddOperation(StoreOperation op);

		/// <summary>
		/// Unregisters the specified operation.
		/// </summary>
		void ReleaseOperation(StoreOperation op);
	}
}
