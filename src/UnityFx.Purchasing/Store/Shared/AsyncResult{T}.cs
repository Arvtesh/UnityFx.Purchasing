// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A simple yieldable asynchronous operatino with a result.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso cref="IAsyncResult"/>
	public class AsyncResult<T> : AsyncResult
	{
		#region data

		private T _result;

		#endregion

		#region interface

		/// <summary>
		/// Returns the operation result. Read only.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if the operation is either not completed or failed/canceled.</exception>
		public T Result
		{
			get
			{
				ThrowIfNotCompleted();
				return _result;
			}
		}

		#endregion

		#region internals

		internal void SetResult(T result)
		{
			if (!TrySetResult(result))
			{
				throw new InvalidOperationException();
			}
		}

		internal bool TrySetResult(T result)
		{
			if (TrySetCompleted())
			{
				_result = result;
				return true;
			}

			return false;
		}

		#endregion
	}
}
