// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A yieldable asynchronous store operation.
	/// </summary>
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	internal abstract class StoreOperation : IStoreOperation, IStoreOperationInfo, IAsyncResult, IEnumerator, IDisposable
	{
		#region data

		private const int _statusDisposedFlag = 1;
		private const int _statusSynchronousFlag = 2;
		private const int _statusRunning = 0;
		private const int _statusCompleted = 4;
		private const int _statusFaulted = 8;
		private const int _statusCanceled = 16;

		private readonly int _id;
		private readonly StoreOperationContainer _owner;

		private static int _lastId;

		private AsyncCallback _asyncCallback;
		private object _asyncState;
		private EventWaitHandle _waitHandle;
		private Exception _exception;

		private volatile int _status;

		#endregion

		#region interface

		protected StoreService Store => _owner.Store;
		protected TraceSource Console => _owner.Store.TraceSource;

		protected StoreOperation(StoreOperationContainer owner, StoreOperationType opType, AsyncCallback asyncCallback, object asyncState, string comment)
		{
			_id = (++_lastId << 4) | (int)opType;
			_owner = owner;
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;

			owner.AddOperation(this);

			var s = GetOperationName();

			if (!string.IsNullOrEmpty(comment))
			{
				s += ": " + comment;
			}

			TraceEvent(TraceEventType.Start, s);
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

		internal void Validate(object owner, StoreOperationType type)
		{
			if ((_id & 0xf) != (int)type)
			{
				throw new ArgumentException("Invalid operation type");
			}

			if (_owner != owner)
			{
				throw new InvalidOperationException("Invalid operation owner");
			}
		}

		internal void Join()
		{
			if (!IsCompleted)
			{
				AsyncWaitHandle.WaitOne();
			}

			if (_exception != null)
			{
#if NET35
				throw _exception;
#else
				ExceptionDispatchInfo.Capture(_exception).Throw();
#endif
			}
		}

		protected bool TrySetCompleted(bool completedSynchronously = false)
		{
			if (TrySetStatus(_statusCompleted, completedSynchronously))
			{
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

		protected void TraceError(string s)
		{
			_owner.Store.TraceSource.TraceEvent(TraceEventType.Error, _id, GetOperationName() + ": " + s);
		}

		protected void TraceException(Exception e)
		{
			_owner.Store.TraceSource.TraceData(TraceEventType.Error, _id, e);
		}

		protected void TraceEvent(TraceEventType eventType, string s)
		{
			_owner.Store.TraceSource.TraceEvent(eventType, _id, s);
		}

		protected void TraceData(TraceEventType eventType, object data)
		{
			_owner.Store.TraceSource.TraceData(eventType, _id, data);
		}

		protected void ThrowIfNotCompletedSuccessfully()
		{
			if ((_status & _statusCompleted) == 0)
			{
				throw new InvalidOperationException("The operation result is not available.", _exception);
			}
		}

		protected void ThrowIfDisposed()
		{
			if ((_status & _statusDisposedFlag) != 0)
			{
				throw new ObjectDisposedException(GetOperationName());
			}
		}

		protected string GetOperationName()
		{
			var result = "UnknownOperation";

			if ((_id & (int)StoreOperationType.Purchase) != 0)
			{
				result = StoreOperationType.Purchase.ToString();
			}
			else if ((_id & (int)StoreOperationType.Fetch) != 0)
			{
				result = StoreOperationType.Fetch.ToString();
			}
			else if ((_id & (int)StoreOperationType.Initialize) != 0)
			{
				result = StoreOperationType.Initialize.ToString();
			}

			return $"{result} ({_id.ToString(CultureInfo.InvariantCulture)})";
		}

		#endregion

		#region IStoreOperationInfo

		public int OperationId => _id;
		public object UserState => _asyncState;

		#endregion

		#region IStoreOperation

		public int Id => _id;
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

				_status |= _statusDisposedFlag;
				_asyncCallback = null;
				_asyncState = null;
				_exception = null;
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
				var s = GetOperationName() + (IsCompletedSuccessfully ? " completed" : " failed");
				TraceEvent(TraceEventType.Stop, s);
			}
			finally
			{
				_owner.ReleaseOperation(this);
				_waitHandle?.Set();
				_asyncCallback?.Invoke(this);
				_asyncCallback = null;
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
