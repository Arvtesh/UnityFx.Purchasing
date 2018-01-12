// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A simple yieledable asynchronous operation.
	/// </summary>
	/// <seealso cref="IAsyncResult"/>
	public class AsyncResult : IAsyncResult, IEnumerator
	{
		#region data

		private const int _statusRunning = 0;
		private const int _statusCompleted = 1;
		private const int _statusFaulted = 2;
		private const int _statusCanceled = 3;

		private Exception _exception;
		private int _status;

		#endregion

		#region interface

		/// <summary>
		/// Returns an <see cref="System.Exception"/> that caused the operation to end prematurely. If the operation completed successfully
		/// or has not yet thrown any exceptions, this will return <c>null</c>. Read only.
		/// </summary>
		/// <seealso cref="IsFaulted"/>
		public Exception Exception => _exception;

		/// <summary>
		/// Returns <see langword="true"/> if the operation has completed successfully, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="IsFaulted"/>
		/// <seealso cref="IsCanceled"/>
		public bool IsCompletedSuccessfully => _status == _statusCompleted;

		/// <summary>
		/// Returns <see langword="true"/> if the operation has failed for any reason, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="Exception"/>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsCanceled"/>
		public bool IsFaulted => _status > _statusCompleted;

		/// <summary>
		/// Returns <see langword="true"/> if the operation has been canceled by user, <see langword="false"/> otherwise. Read only.
		/// </summary>
		/// <seealso cref="IsCompletedSuccessfully"/>
		/// <seealso cref="IsFaulted"/>
		public bool IsCanceled => _status == _statusCanceled;

		/// <summary>
		/// Throws exception if the operation has failed.
		/// </summary>
		protected void ThrowIfNotCompleted()
		{
			if (_status != _statusCompleted)
			{
				throw new InvalidOperationException("The operation result is not available.", _exception);
			}
		}

		#endregion

		#region internals

		internal void SetCanceled()
		{
			if (!TrySetCanceled())
			{
				throw new InvalidOperationException();
			}
		}

		internal bool TrySetCanceled()
		{
			return TrySetStatus(_statusCanceled);
		}

		internal void SetException(Exception e)
		{
			if (!TrySetException(e))
			{
				throw new InvalidOperationException();
			}
		}

		internal bool TrySetException(Exception e)
		{
			var status = e is OperationCanceledException ? _statusCanceled : _statusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				return true;
			}

			return false;
		}

		internal void SetCompleted()
		{
			if (!TrySetCompleted())
			{
				throw new InvalidOperationException();
			}
		}

		internal bool TrySetCompleted()
		{
			return TrySetStatus(_statusCompleted);
		}

		#endregion

		#region IAsyncResult

		/// <inheritdoc/>
		WaitHandle IAsyncResult.AsyncWaitHandle => throw new NotSupportedException();

		/// <inheritdoc/>
		object IAsyncResult.AsyncState => null;

		/// <inheritdoc/>
		bool IAsyncResult.CompletedSynchronously => false;

		/// <inheritdoc/>
		public bool IsCompleted => _status > _statusRunning;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		object IEnumerator.Current => null;

		/// <inheritdoc/>
		bool IEnumerator.MoveNext() => _status == _statusRunning;

		/// <inheritdoc/>
		void IEnumerator.Reset() => throw new NotSupportedException();

		#endregion

		#region implementation

		private bool TrySetStatus(int newStatus)
		{
			if (_status < _statusCompleted)
			{
				_status = newStatus;
				return true;
			}

			return false;
		}

		#endregion
	}
}
