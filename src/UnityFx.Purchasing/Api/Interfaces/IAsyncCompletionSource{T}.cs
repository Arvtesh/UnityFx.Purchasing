// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents the producer side of a an synchronous operation unbound to a delegate.
	/// </summary>
	public interface IAsyncCompletionSource<T>
	{
		/// <summary>
		/// Sets result of the operation (and transitions it to completed state).
		/// </summary>
		void SetResult(T result);

		/// <summary>
		/// Sets exception and transitions the operation into failed state.
		/// </summary>
		void SetException(Exception e);
	}
}
