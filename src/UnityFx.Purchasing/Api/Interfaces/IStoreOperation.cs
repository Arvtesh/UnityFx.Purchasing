// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A yieldable store operation with status information.
	/// </summary>
	/// <seealso cref="IStoreOperation{T}"/>
	public interface IStoreOperation : IAsyncOperation
	{
		/// <summary>
		/// Returns an operation identifier. Read only.
		/// </summary>
		/// <value>An unique operation identifier.</value>
		int Id { get; }
	}
}
