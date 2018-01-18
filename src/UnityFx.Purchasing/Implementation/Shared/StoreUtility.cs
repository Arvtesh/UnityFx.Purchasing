﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates identifiers for <see cref="TraceSource"/> methods.
	/// </summary>
	internal enum StoreOperationId
	{
		Default,
		Initialize,
		Fetch,
		Purchase
	}

	/// <summary>
	/// Helpers.
	/// </summary>
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
