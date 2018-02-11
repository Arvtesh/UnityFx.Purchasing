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
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A yieldable asynchronous store operation.
	/// </summary>
	internal abstract class StoreOperation : AsyncResult, IStoreOperationInfo
	{
		#region data

		private const int _typeMask = 0x3;

		private readonly int _id;
		private readonly string _name;
		private readonly IStoreOperationOwner _owner;

		private static int _lastId;

		#endregion

		#region interface

		internal int Id => _id;

		protected StoreService Store => _owner.Store;

		protected StoreOperation(IStoreOperationOwner owner, StoreOperationType opType, AsyncCallback asyncCallback, object asyncState, string comment)
			: base(asyncCallback, asyncState)
		{
			_id = (++_lastId << 2) | (int)opType;
			_name = $"{opType.ToString()} ({_id.ToString(CultureInfo.InvariantCulture)})";
			_owner = owner;
			_owner.AddOperation(this);

			TraceStart(comment);
		}

		internal void Validate(object owner, StoreOperationType type)
		{
			if ((_id & _typeMask) != (int)type)
			{
				throw new ArgumentException("Invalid operation type");
			}

			if (_owner != owner)
			{
				throw new InvalidOperationException("Invalid operation owner");
			}
		}

		protected void TraceError(string s)
		{
			_owner.TraceSource.TraceEvent(TraceEventType.Error, _id, _name + ": " + s);
		}

		protected void TraceException(Exception e)
		{
			_owner.TraceSource.TraceData(TraceEventType.Error, _id, e);
		}

		protected void TraceEvent(TraceEventType eventType, string s)
		{
			_owner.TraceSource.TraceEvent(eventType, _id, s);
		}

		protected void TraceData(TraceEventType eventType, object data)
		{
			_owner.TraceSource.TraceData(eventType, _id, data);
		}

		#endregion

		#region AsyncResult

		protected override void OnStatusChanged(AsyncOperationStatus status)
		{
			base.OnStatusChanged(status);
			TraceStop(status);
		}

		protected override void OnCompleted()
		{
			base.OnCompleted();
			_owner.ReleaseOperation(this);
		}

		#endregion

		#region IStoreOperationInfo

		public int OperationId => _id;
		public object UserState => AsyncState;

		#endregion

		#region implementation

		private void TraceStart(string comment)
		{
			var s = _name;

			if (!string.IsNullOrEmpty(comment))
			{
				s += ": " + comment;
			}

			_owner.TraceSource.TraceEvent(TraceEventType.Start, _id, s);
		}

		private void TraceStop(AsyncOperationStatus status)
		{
			if (status == AsyncOperationStatus.RanToCompletion)
			{
				_owner.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " completed");
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				_owner.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " faulted");
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				_owner.TraceSource.TraceEvent(TraceEventType.Stop, _id, _name + " canceled");
			}
		}

		#endregion
	}
}
