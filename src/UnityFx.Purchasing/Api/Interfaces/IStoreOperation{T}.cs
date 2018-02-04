// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	///  Extends an <see cref="IStoreOperation"/> interface with a result value.
	/// </summary>
	/// <seealso cref="IAsyncResult"/>
	/// <seealso cref="IStoreOperation"/>
	public interface IStoreOperation<out T> : IStoreOperation, IAsyncOperation<T>
	{
	}
}
