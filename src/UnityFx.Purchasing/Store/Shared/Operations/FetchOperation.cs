// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A fetch operation.
	/// </summary>
	internal class FetchOperation : StoreOperation<object>
	{
		#region interface

		public FetchOperation(StoreOperationContainer parent, TraceEventId eventId)
			: base(parent, eventId, null, null)
		{
			if (EventId == TraceEventId.Fetch)
			{
				Store.InvokeFetchInitiated();
			}
			else
			{
				Store.InvokeInitializeInitiated();
			}
		}

		public void SetCompleted()
		{
			if (TrySetResult(null))
			{
				if (EventId == TraceEventId.Fetch)
				{
					Store.InvokeFetchCompleted();
				}
				else
				{
					Store.InvokeInitializeCompleted();
				}
			}
		}

		public void SetFailed(StoreFetchError reason)
		{
			TraceError(reason.ToString());

			if (TrySetException(new StoreFetchException(reason)))
			{
				if (EventId == TraceEventId.Fetch)
				{
					Store.InvokeFetchFailed(reason, null);
				}
				else
				{
					Store.InvokeInitializeFailed(reason, null);
				}
			}
		}

		public void SetFailed(Exception e)
		{
			TraceException(e);

			if (TrySetException(e))
			{
				var reason = e is StoreFetchException sfe ? sfe.Reason : StoreFetchError.Unknown;

				if (EventId == TraceEventId.Fetch)
				{
					Store.InvokeFetchFailed(reason, e);
				}
				else
				{
					Store.InvokeInitializeFailed(reason, e);
				}
			}
		}

		#endregion
	}
}
