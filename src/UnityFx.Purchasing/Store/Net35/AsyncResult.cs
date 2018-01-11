// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A simple asynchronous operation that can be yieled.
	/// </summary>
	/// <seealso cref="IAsyncResult"/>
	public class AsyncResult : IAsyncResult, IEnumerator
	{
		#region data

		private Exception _exception;
		private int _status = StatusInitialized;

		#endregion

		#region interface

		internal const int StatusInitialized = -1;
		internal const int StatusRunning = 0;
		internal const int StatusCompleted = 1;
		internal const int StatusFaulted = 2;
		internal const int StatusCanceled = 3;

		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <c>null</c>. Read only.
		/// </summary>
		public Exception Exception => _exception;

		/// <summary>
		/// Returns <c>true</c> if the operation has completed successfully, <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsCompletedSuccessfully => _status == StatusCompleted;

		/// <summary>
		/// Returns <c>true</c> if the operation has failed for any reason, <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsFaulted => _status > StatusCompleted;

		/// <summary>
		/// Returns <c>true</c> if the operation has been canceled by user, <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsCanceled => _status == StatusCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncResult"/> class.
		/// </summary>
		public AsyncResult()
		{
		}

		/// <summary>
		/// Transitions the operation into the canceled state.
		/// </summary>
		internal void SetCanceled()
		{
			if (!TrySetCanceled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the canceled state.
		/// </summary>
		internal bool TrySetCanceled()
		{
			if (TrySetStatus(StatusCanceled))
			{
				OnCompleted();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Transitions the operation into the faulted (or canceled) state.
		/// </summary>
		internal void SetException(Exception e)
		{
			if (!TrySetException(e))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the faulted (or canceled) state.
		/// </summary>
		internal bool TrySetException(Exception e)
		{
			var status = e is OperationCanceledException ? StatusCanceled : StatusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				OnCompleted();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Transitions the operation into the completed state.
		/// </summary>
		internal void SetCompleted()
		{
			if (!TrySetCompleted())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the completed state.
		/// </summary>
		internal bool TrySetCompleted()
		{
			if (TrySetStatus(StatusCompleted))
			{
				OnCompleted();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Finished the operation (if it is not already finished). Do not use this method unless absolutely needed.
		/// </summary>
		protected bool TrySetStatus(int newStatus)
		{
			if (_status < StatusCompleted && newStatus > _status)
			{
				_status = newStatus;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Throws exception if the operation has failed.
		/// </summary>
		protected void ThrowIfFaulted()
		{
			if (_status > StatusCompleted)
			{
				throw new InvalidOperationException("The operation result is not available.", _exception);
			}
		}

		/// <summary>
		/// Updates the operation state. Called by <see cref="MoveNext"/>. Default implementation does nothing.
		/// </summary>
		/// <remarks>
		/// Do not reference public class methods and properties in this method (except <see cref="SetCompleted"/>)
		/// because their implementation may reference <see cref="MoveNext"/> and cause endless recursion.
		/// </remarks>
		/// <seealso cref="OnCompleted()"/>
		protected virtual void OnUpdate()
		{
		}

		/// <summary>
		/// Called when the operation has completed (either successfully or not). Default implementation does nothing.
		/// </summary>
		/// <seealso cref="OnUpdate()"/>
		protected virtual void OnCompleted()
		{
		}

		#endregion

		#region IAsyncResult

		/// <inheritdoc/>
		public WaitHandle AsyncWaitHandle => throw new NotSupportedException();

		/// <inheritdoc/>
		public object AsyncState => null;

		/// <inheritdoc/>
		public bool CompletedSynchronously => false;

		/// <inheritdoc/>
		public bool IsCompleted => _status > StatusRunning;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		public object Current => null;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_status > StatusRunning)
			{
				// The operation has completed.
				return false;
			}
			else
			{
				// If this is the first time MoveNext() is called, switch status to Running.
				if (_status == StatusInitialized)
				{
					TrySetStatus(StatusRunning);
				}

				// The operation is pending.
				try
				{
					OnUpdate();
				}
				catch (Exception e)
				{
					TrySetException(e);
				}

				return _status == StatusRunning;
			}
		}

		/// <inheritdoc/>
		public void Reset() => throw new NotSupportedException();

		#endregion
	}
}
