// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic disposable store operation that logs start/end events.
	/// </summary>
	internal abstract class StoreOperation<T> : AsyncResult<T>, IDisposable
	{
		#region data

		private readonly TraceSource _console;
		private readonly TraceEventId _traceEvent;
		private readonly string _args;
#if !NET35
		private TaskCompletionSource<T> _tcs;
#endif
		private bool _disposed;

		#endregion

		#region interface

		protected bool IsDisposed => _disposed;

#if !NET35
		public Task<T> Task
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

		public StoreOperation(TraceSource console, TraceEventId eventId, string comment, string args)
		{
			_console = console;
			_traceEvent = eventId;
			_args = args;

			var s = eventId.ToString();

			if (!string.IsNullOrEmpty(comment))
			{
				s += " (" + comment + ')';
			}

			if (!string.IsNullOrEmpty(args))
			{
				s += ": " + args;
			}

			_console.TraceEvent(TraceEventType.Start, (int)eventId, s);
		}

		public new void SetResult(T result)
		{
			base.SetResult(result);

#if !NET35
			if (_tcs != null)
			{
				_tcs.SetResult(result);
			}
#endif
		}

		public new void TrySetResult(T result)
		{
			base.TrySetResult(result);

#if !NET35
			if (_tcs != null)
			{
				_tcs.TrySetResult(result);
			}
#endif
		}

		public new void SetException(Exception e)
		{
			base.SetException(e);

#if !NET35
			if (_tcs != null)
			{
				_tcs.SetException(e);
			}
#endif
		}

		public new void TrySetException(Exception e)
		{
			base.TrySetException(e);

#if !NET35
			if (_tcs != null)
			{
				_tcs.TrySetException(e);
			}
#endif
		}

		public new void SetCanceled()
		{
			base.SetCanceled();

#if !NET35
			if (_tcs != null)
			{
				_tcs.SetCanceled();
			}
#endif
		}

		public new void TrySetCanceled()
		{
			base.TrySetCanceled();

#if !NET35
			if (_tcs != null)
			{
				_tcs.TrySetCanceled();
			}
#endif
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				var s = _traceEvent.ToString() + (IsCompletedSuccessfully ? " completed" : " failed");

				if (!string.IsNullOrEmpty(_args))
				{
					s += ": " + _args;
				}

				_console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, s);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
