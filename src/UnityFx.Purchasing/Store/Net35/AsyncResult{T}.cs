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
	/// <seealso cref="AsyncResult"/>
	public class AsyncResult<T> : AsyncResult
	{
		#region data

		private T _result;

		#endregion

		#region interface

		/// <summary>
		/// Returns the result value of this operation. Read only.
		/// </summary>
		public T Result
		{
			get
			{
				ThrowIfFaulted();
				return _result;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult{T}"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Transitions the operation into the completed state.
		/// </summary>
		internal void SetResult(T result)
		{
			if (!TrySetResult(result))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the completed state.
		/// </summary>
		internal bool TrySetResult(T result)
		{
			if (TrySetStatus(StatusCompleted))
			{
				_result = result;
				OnCompleted();
				return true;
			}

			return false;
		}

		#endregion
	}
}
