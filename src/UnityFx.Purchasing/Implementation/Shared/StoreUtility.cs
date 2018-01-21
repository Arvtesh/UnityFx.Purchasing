// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	internal enum AsyncPatternId
	{
		Eap,
		Apm,
		Tap
	}

	internal enum StoreOperationId
	{
		Initialize = 1,
		Fetch = 2,
		Purchase = 3
	}

	internal static class StoreUtility
	{
		internal static void TraceError(this TraceSource traceSource, StoreOperationId opId, string s)
		{
			traceSource.TraceEvent(TraceEventType.Error, (int)opId, opId.ToString() + " error: " + s);
		}

		internal static void TraceException(this TraceSource traceSource, StoreOperationId opId, Exception e, TraceEventType eventType = TraceEventType.Error)
		{
			if (e != null)
			{
				traceSource.TraceData(eventType, (int)opId, e);
			}
		}
	}
}
