// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A yieldable asynchronous store operation with a result.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	internal abstract class StoreOperation<T> : IStoreOperation<T>, IEnumerator
	{
		#region data

		private const int _statusSynchronousFlag = 1;
		private const int _statusRunning = 0;
		private const int _statusCompleted = 2;
		private const int _statusFaulted = 4;
		private const int _statusCanceled = 8;

		private readonly StoreOperationContainer _owner;
		private readonly TraceEventId _traceEvent;
		private readonly string _args;

		private readonly AsyncCallback _asyncCallback;
		private readonly object _asyncState;

		private EventWaitHandle _waitHandle;
		private Exception _exception;

		private Action<IStoreOperation> _continuation;
		private T _result;

		private volatile int _status;

		#endregion

		#region interface

		protected StoreOperationContainer Owner => _owner;
		protected StoreService Store => _owner.Store;
		protected TraceSource Console => _owner.Store.TraceSource;
		protected TraceEventId EventId => _traceEvent;

		protected StoreOperation(StoreOperationContainer owner, T result, AsyncCallback asyncCallback, object asyncState)
		{
			_owner = owner;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
			_result = result;
		}

		protected StoreOperation(StoreOperationContainer owner, TraceEventId eventId, string comment, string args, AsyncCallback asyncCallback, object asyncState)
		{
			_owner = owner;
			_traceEvent = eventId;
			_args = args;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;

			var s = eventId.ToString();

			if (!string.IsNullOrEmpty(comment))
			{
				s += " (" + comment + ')';
			}

			if (!string.IsNullOrEmpty(args))
			{
				s += ": " + args;
			}

			owner.AddOperation(this);

			Console.TraceEvent(TraceEventType.Start, (int)eventId, s);
		}

		internal void WaitUntilCompleted()
		{
			if (IsCompleted)
			{
				_waitHandle?.Close();
			}
			else
			{
				var waitHandle = AsyncWaitHandle;

				waitHandle.WaitOne();
				waitHandle.Close();
			}
		}

		internal void ThrowIfInvalidOwner(object owner)
		{
			if (_owner != owner)
			{
				throw new InvalidOperationException("Invalid operation owner");
			}
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

		protected bool TrySetResult(T result, bool completedSynchronously = false)
		{
			if (TrySetStatus(_statusCompleted, completedSynchronously))
			{
				_result = result;
				OnCompleted();
				return true;
			}

			return false;
		}

		protected bool TrySetException(Exception e, bool completedSynchronously = false)
		{
			var status = e is OperationCanceledException ? _statusCanceled : _statusFaulted;

			if (TrySetStatus(status, completedSynchronously))
			{
				_exception = e;
				OnCompleted();
				return true;
			}

			return false;
		}

		protected bool TrySetCanceled(bool completedSynchronously = false)
		{
			if (TrySetStatus(_statusCanceled, completedSynchronously))
			{
				OnCompleted();
				return true;
			}

			return false;
		}

		#endregion

		#region IAsyncOperation

		/// <inheritdoc/>
		public Exception Exception => _exception;

		/// <inheritdoc/>
		public bool IsCompletedSuccessfully => (_status & _statusCompleted) != 0;

		/// <inheritdoc/>
		public bool IsFaulted => _status >= _statusFaulted;

		/// <inheritdoc/>
		public bool IsCanceled => (_status & _statusCanceled) != 0;

		/// <inheritdoc/>
		public T Result
		{
			get
			{
				if ((_status & _statusCompleted) == 0)
				{
					throw new InvalidOperationException("The operation result is not available.", _exception);
				}

				return _result;
			}
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
		public bool CompletedSynchronously => (_status & _statusSynchronousFlag) != 0;

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

		private void OnCompleted()
		{
			try
			{
				var s = _traceEvent.ToString() + (IsCompletedSuccessfully ? " completed" : " failed");

				if (!string.IsNullOrEmpty(_args))
				{
					s += ": " + _args;
				}

				Console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, s);
			}
			finally
			{
				_owner.ReleaseOperation(this);
				_waitHandle?.Set();
				_asyncCallback?.Invoke(this);
				_continuation?.Invoke(this);
			}
		}

		private bool TrySetStatus(int newStatus, bool completedSynchronously)
		{
			if (_status < _statusCompleted)
			{
				if (completedSynchronously)
				{
					newStatus |= _statusSynchronousFlag;
				}

				return Interlocked.CompareExchange(ref _status, newStatus, _statusRunning) == _statusRunning;
			}

			return false;
		}

		#endregion
	}
}
