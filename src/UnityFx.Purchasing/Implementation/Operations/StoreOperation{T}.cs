// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	internal abstract class StoreOperation<T> : AsyncResult<T>
	{
		#region data

		private readonly int _id;
		private readonly string _name;
		private readonly StoreService _store;

		private static int _lastId;

		#endregion

		#region interface

		internal int Id => _id;

		protected StoreService Store => _store;

		protected StoreOperation(StoreService store, StoreOperationType opType, object asyncState, string comment)
			: base(null, asyncState)
		{
			_id = ++_lastId;
			_name = $"{opType.ToString()} ({_id.ToString(CultureInfo.InvariantCulture)})";
			_store = store;

			TraceStart(comment);
		}

		protected void TraceError(string s)
		{
			_store.TraceSource.TraceEvent(TraceEventType.Error, _id, _name + ": " + s);
		}

		protected void TraceException(Exception e)
		{
			_store.TraceSource.TraceData(TraceEventType.Error, _id, e);
		}

		protected void TraceEvent(TraceEventType eventType, string s)
		{
			_store.TraceSource.TraceEvent(eventType, _id, s);
		}

		protected void TraceData(TraceEventType eventType, object data)
		{
			_store.TraceSource.TraceData(eventType, _id, data);
		}

		#endregion

		#region AsyncResult

		protected override void OnStatusChanged(AsyncOperationStatus status)
		{
			base.OnStatusChanged(status);
			TraceStop(status);
		}

		#endregion

		#region implementation

		private void TraceStart(string comment)
		{
			var s = _name;

			if (!string.IsNullOrEmpty(comment))
			{
				s += ": " + comment;
			}

			_store.TraceSource.TraceEvent(TraceEventType.Start, _id, s);
		}

		private void TraceStop(AsyncOperationStatus status)
		{
			if (status == AsyncOperationStatus.RanToCompletion)
			{
				_store.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " completed");
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				_store.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " faulted");
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				_store.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " canceled");
			}
		}

		#endregion
	}
}
