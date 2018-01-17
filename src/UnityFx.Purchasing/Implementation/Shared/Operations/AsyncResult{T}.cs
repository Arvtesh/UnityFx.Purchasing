﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A simple yieldable asynchronous operatino with a result.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	internal class AsyncResult<T> : IStoreOperation<T>, IEnumerator
	{
		#region data

		private const int _statusRunning = 0;
		private const int _statusCompleted = 1;
		private const int _statusFaulted = 2;
		private const int _statusCanceled = 3;

		private static AsyncResult<T> _completed;

		private readonly AsyncCallback _asyncCallback;
		private readonly object _asyncState;

		private bool _completedSynchronously;
		private EventWaitHandle _waitHandle;
		private Exception _exception;

		private Action<IStoreOperation> _continuation;
		private T _result;

		private volatile int _status;

#if !NET35
		private TaskCompletionSource<T> _tcs;
#endif

		#endregion

		#region interface

		/// <summary>
		/// Returns a completed operation. Read only.
		/// </summary>
		public static AsyncResult<T> Completed
		{
			get
			{
				if (_completed == null)
				{
					_completed = new AsyncResult<T>(default(T));
				}

				return _completed;
			}
		}

		#endregion

		#region internals

#if !NET35
		internal Task<T> Task
		{
			get
			{
				if (_tcs == null)
				{
					_tcs = new TaskCompletionSource<T>();
				}

				return _tcs.Task;
			}
		}
#endif

		internal AsyncResult()
		{
		}

		internal AsyncResult(AsyncCallback asyncCallback, object asyncState)
		{
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
		}

		internal AsyncResult(T result)
		{
			_status = _statusCompleted;
			_result = result;
		}

		internal void ContinueWith(Action<IStoreOperation> continuation)
		{
			Debug.Assert(continuation != null);

			if (_status > _statusRunning)
			{
				continuation(this);
			}
			else
			{
				_continuation += continuation;
			}
		}

		protected bool TrySetResult(T result)
		{
			if (TrySetStatus(_statusCompleted))
			{
				_result = result;
				OnCompleted();
#if NET35
				return true;
#else
				return _tcs?.TrySetResult(result) ?? true;
#endif
			}

			return false;
		}

		protected bool TrySetException(Exception e)
		{
			var status = e is OperationCanceledException ? _statusCanceled : _statusFaulted;

			if (TrySetStatus(status))
			{
				_exception = e;
				OnCompleted();
#if NET35
				return true;
#else
				return _tcs?.TrySetException(e) ?? true;
#endif
			}

			return false;
		}

		protected bool TrySetCanceled()
		{
			if (TrySetStatus(_statusCanceled))
			{
				OnCompleted();
#if NET35
				return true;
#else
				return _tcs?.TrySetCanceled() ?? true;
#endif
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => _status == _statusCompleted;

		/// <inheritdoc/>
		public bool IsFaulted => _status > _statusCompleted;

		/// <inheritdoc/>
		public bool IsCanceled => _status == _statusCanceled;

		/// <inheritdoc/>
		public T Result
		{
			get
			{
				if (_status != _statusCompleted)
				{
					throw new InvalidOperationException("The operation result is not available.", _exception);
				}

				return _result;
			}
		}

		/// <summary>
		/// Called when the operation has been completed (with or without exceptions).
		/// </summary>
		protected virtual void OnCompleted()
		{
			_waitHandle?.Set();
			_continuation?.Invoke(this);
		}

		#endregion

		#region IAsyncResult

		/// <inheritdoc/>
		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (_waitHandle == null)
				{
					var done = IsCompleted;
					var mre = new ManualResetEvent(done);

					if (Interlocked.CompareExchange(ref _waitHandle, mre, null) != null)
					{
						// Another thread created this object's event; dispose the event we just created.
						mre.Close();
					}
					else if (!done && IsCompleted)
					{
						// We published the event as unset, but the operation has subsequently completed;
						// set the event state properly so that callers do not deadlock.
						_waitHandle.Set();
					}
				}

				return _waitHandle;
			}
		}

		/// <inheritdoc/>
		public object AsyncState => _asyncState;

		/// <inheritdoc/>
		public bool CompletedSynchronously => _completedSynchronously;

		/// <inheritdoc/>
		public bool IsCompleted => _status > _statusRunning;

		#endregion

		#region IEnumerator

		/// <inheritdoc/>
		public object Current => null;

		/// <inheritdoc/>
		public bool MoveNext() => _status == _statusRunning;

		/// <inheritdoc/>
		public void Reset() => throw new NotSupportedException();

		#endregion

		#region implementation

		private bool TrySetStatus(int newStatus)
		{
			if (_status < _statusCompleted)
			{
				return Interlocked.CompareExchange(ref _status, newStatus, _statusRunning) == _statusRunning;
			}

			return false;
		}

		#endregion
	}
}
