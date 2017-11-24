// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents a store operation.
	/// </summary>
	internal abstract class StoreOperation<T> : TaskCompletionSource<T>, IDisposable
	{
		#region data

		private readonly TraceSource _console;
		private readonly StoreService.TraceEventId _traceEvent;
		private readonly string _args;
		private bool _disposed;

		#endregion

		#region interface

		public bool IsDisposed => _disposed;

		public StoreOperation(TraceSource console, StoreService.TraceEventId eventId, string comment, string args)
		{
			_console = console;
			_traceEvent = eventId;
			_args = args;

			var s = _traceEvent.ToString();

			if (!string.IsNullOrEmpty(comment))
			{
				s += " (" + comment + ')';
			}

			if (!string.IsNullOrEmpty(args))
			{
				s += ": " + args;
			}

			_console.TraceEvent(TraceEventType.Start, (int)_traceEvent, s);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				if (Task.Status == TaskStatus.RanToCompletion)
				{
					if (string.IsNullOrEmpty(_args))
					{
						_console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, _traceEvent.ToString() + " completed");
					}
					else
					{
						_console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, _traceEvent.ToString() + " completed: " + _args);
					}
				}
				else
				{
					if (string.IsNullOrEmpty(_args))
					{
						_console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, _traceEvent.ToString() + " failed");
					}
					else
					{
						_console.TraceEvent(TraceEventType.Stop, (int)_traceEvent, _traceEvent.ToString() + " failed: " + _args);
					}
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
