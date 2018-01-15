// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

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
			if (EventId == TraceEventId.Fetch)
			{
				Store.InvokeFetchCompleted();
			}
			else
			{
				Store.InvokeInitializeCompleted();
			}

			TrySetResult(null);
		}

		public void SetFailed(StoreFetchError reason)
		{
			if (EventId == TraceEventId.Fetch)
			{
				Store.InvokeFetchFailed(reason, null);
			}
			else
			{
				Store.InvokeInitializeFailed(reason, null);
			}

			TrySetException(new StoreFetchException(reason));
		}

		public void SetFailed(Exception e)
		{
			if (e is StoreFetchException sfe)
			{
				if (EventId == TraceEventId.Fetch)
				{
					Store.InvokeFetchFailed(sfe.Reason, e);
				}
				else
				{
					Store.InvokeInitializeFailed(sfe.Reason, e);
				}

				TrySetException(e);
			}
			else
			{
				if (EventId == TraceEventId.Fetch)
				{
					Store.InvokeFetchFailed(StoreFetchError.Unknown, e);
				}
				else
				{
					Store.InvokeInitializeFailed(StoreFetchError.Unknown, e);
				}

				TrySetException(new StoreFetchException(StoreFetchError.Unknown, e));
			}
		}

		#endregion
	}
}
