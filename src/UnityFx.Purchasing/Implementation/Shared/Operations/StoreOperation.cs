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
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	internal class StoreOperation : IStoreOperation, IEnumerator
	{
		#region data

		private const int _statusDisposedFlag = 1;
		private const int _statusSynchronousFlag = 2;
		private const int _statusRunning = 0;
		private const int _statusCompleted = 4;
		private const int _statusFaulted = 8;
		private const int _statusCanceled = 16;

		private readonly StoreOperationContainer _owner;
		private readonly StoreOperationId _type;
		private readonly string _args;

		private StoreOperation _continuationOp;
		private AsyncCallback _asyncCallback;
		private object _asyncState;
		private EventWaitHandle _waitHandle;
		private Exception _exception;
		private object _result;

		private volatile int _status;

		#endregion

		#region interface

		internal StoreOperationId Type => _type;
		internal object Owner => _owner;
		protected StoreService Store => _owner.Store;
		protected TraceSource Console => _owner.Store.TraceSource;

		private StoreOperation(StoreOperation parentOp, AsyncCallback asyncCallback, object asyncState)
		{
			_owner = parentOp._owner;
			_type = parentOp._type;
			_exception = parentOp._exception;
			_result = parentOp._result;
			_status = parentOp._status;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
		}

		private StoreOperation(StoreOperationContainer owner, object result, StoreOperationId opId, AsyncCallback asyncCallback, object asyncState)
		{
			_owner = owner;
			_type = opId;
			_result = result;
			_status = _statusCompleted | _statusSynchronousFlag;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
		}

		protected StoreOperation(StoreOperationContainer owner, StoreOperationId opId, AsyncCallback asyncCallback, object asyncState, string comment, string args)
		{
			_owner = owner;
			_type = opId;
			_args = args;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;

			var s = opId.ToString();

			if (!string.IsNullOrEmpty(comment))
			{
				s += " (" + comment + ')';
			}

			if (!string.IsNullOrEmpty(args))
			{
				s += ": " + args;
			}

			owner.AddOperation(this);

			Console.TraceEvent(TraceEventType.Start, (int)opId, s);
		}

		internal static StoreOperation GetCompletedOperation(StoreOperationContainer owner, StoreOperationId opId, AsyncCallback asyncCallback, object asyncState)
		{
			var result = new StoreOperation(owner, null, opId, null, asyncState);

			if (result.IsCompleted)
			{
				asyncCallback?.Invoke(result);
			}

			return result;
		}

		internal void AddCompletionHandler(AsyncCallback continuation)
		{
			if (IsCompleted)
			{
				continuation(this);
			}
			else
			{
				_asyncCallback += continuation;
			}
		}

		internal StoreOperation ContinueWith(AsyncCallback continuation, object asyncState)
		{
			var op = new StoreOperation(this, continuation, asyncState);

			if (op.IsCompleted)
			{
				continuation?.Invoke(op);
			}
			else
			{
				var parentOp = this;

				while (parentOp._continuationOp != null)
				{
					parentOp = parentOp._continuationOp;
				}

				parentOp._continuationOp = op;
			}

			return op;
		}

		protected bool TrySetResult(object result, bool completedSynchronously = false)
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

		protected object GetResult()
		{
			if ((_status & _statusCompleted) == 0)
			{
				throw new InvalidOperationException("The operation result is not available.", _exception);
			}

			return _result;
		}

		protected void ThrowIfDisposed()
		{
			if ((_status & _statusDisposedFlag) != 0)
			{
				throw new ObjectDisposedException(_type.ToString());
			}
		}

		#endregion

		#region IStoreOperation

		public event AsyncCallback Completed
		{
			add
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				AddCompletionHandler(value);
			}
			remove
			{
				_asyncCallback -= value;
			}
		}

		public Exception Exception => _exception;
		public bool IsCompletedSuccessfully => (_status & _statusCompleted) != 0;
		public bool IsFaulted => _status >= _statusFaulted;
		public bool IsCanceled => (_status & _statusCanceled) != 0;

		#endregion

		#region IAsyncResult

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				ThrowIfDisposed();

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

		public object AsyncState => _asyncState;
		public bool CompletedSynchronously => (_status & _statusSynchronousFlag) != 0;
		public bool IsCompleted => _status > _statusRunning;

		#endregion

		#region IEnumerator

		public object Current => null;
		public bool MoveNext() => _status == _statusRunning;
		public void Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if ((_status & _statusDisposedFlag) == 0)
			{
				if (!IsCompleted)
				{
					throw new InvalidOperationException("Cannot dispose non-completed operation.");
				}

				_status = _statusDisposedFlag;
				_asyncCallback = null;
				_asyncState = null;
				_exception = null;
				_result = null;
				_waitHandle?.Close();
				_waitHandle = null;
			}
		}

		#endregion

		#region implementation

		private void OnCompleted()
		{
			try
			{
				var s = _type.ToString() + (IsCompletedSuccessfully ? " completed" : " failed");

				if (!string.IsNullOrEmpty(_args))
				{
					s += ": " + _args;
				}

				Console.TraceEvent(TraceEventType.Stop, (int)_type, s);
			}
			finally
			{
				_owner.ReleaseOperation(this);

				SignalCompletionEvents();
				UpdateContinuation(this);
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

		private void SignalCompletionEvents()
		{
			_waitHandle?.Set();
			_asyncCallback?.Invoke(this);
			_asyncCallback = null;
		}

		private void UpdateContinuation(StoreOperation op)
		{
			if (TrySetStatus(op._status, false))
			{
				_exception = op._exception;
				_result = op._result;

				SignalCompletionEvents();
			}

			_continuationOp?.UpdateContinuation(op);
			_continuationOp = null;
		}

		#endregion
	}
}
