// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic disposable store operation that logs start/end events.
	/// </summary>
	internal abstract class StoreOperation<T> : TaskCompletionSource<T>, IDisposable
	{
		#region data

		private readonly TraceSource _console;
		private readonly TraceEventId _traceEvent;
		private readonly string _args;
		private bool _disposed;

		#endregion

		#region interface

		protected bool IsDisposed => _disposed;

		public StoreOperation(TraceSource console, TraceEventId eventId, string comment, string args)
		{
			_console = console;
			_traceEvent = eventId;
			_args = args;

			StoreUtility.TraceOperationBegin(console, eventId, comment, args);
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
					StoreUtility.TraceOperationComplete(_console, _traceEvent, _args);
				}
				else
				{
					StoreUtility.TraceOperationFailed(_console, _traceEvent, _args);
				}
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
