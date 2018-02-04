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
	/// <seealso href="https://blogs.msdn.microsoft.com/nikos/2011/03/14/how-to-implement-the-iasyncresult-design-pattern/"/>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	internal abstract class StoreOperation : AsyncResult, IStoreOperation, IStoreOperationInfo
	{
		#region data

		private const int _typeMask = 0x3;

		private readonly int _id;
		private readonly IStoreOperationOwner _owner;

		private static int _lastId;

		#endregion

		#region interface

		protected StoreService Store => _owner.Store;

		protected StoreOperation(IStoreOperationOwner owner, StoreOperationType opType, AsyncCallback asyncCallback, object asyncState, string comment)
			: base(asyncCallback, asyncState)
		{
			_id = (++_lastId << 2) | (int)opType;
			_owner = owner;

			owner.AddOperation(this);

			var s = GetOperationName();

			if (!string.IsNullOrEmpty(comment))
			{
				s += ": " + comment;
			}

			TraceEvent(TraceEventType.Start, s);
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
			_owner.TraceSource.TraceEvent(TraceEventType.Error, _id, GetOperationName() + ": " + s);
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

		protected override void OnCompleted()
		{
			try
			{
				var s = GetOperationName() + (IsCompletedSuccessfully ? " completed" : " failed");

				TraceEvent(TraceEventType.Stop, s);

				base.OnCompleted();
			}
			finally
			{
				_owner.ReleaseOperation(this);
			}
		}

		#endregion

		#region IStoreOperationInfo

		public int OperationId => _id;
		public object UserState => AsyncState;

		#endregion

		#region IStoreOperation

		public int Id => _id;

		#endregion

		#region implementation

		private string GetOperationName()
		{
			var result = (StoreOperationType)(_id & _typeMask);
			return $"{result.ToString()} ({_id.ToString(CultureInfo.InvariantCulture)})";
		}

		#endregion
	}
}
