// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates identifiers for <see cref="TraceSource"/> methods.
	/// </summary>
	internal enum TraceEventId
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
		internal static void TraceError(this TraceSource traceSource, TraceEventId eventId, string s)
		{
			traceSource.TraceEvent(TraceEventType.Error, (int)eventId, eventId.ToString() + " error: " + s);
		}

		internal static void TraceException(this TraceSource traceSource, TraceEventId eventId, Exception e)
		{
			if (e != null)
			{
				traceSource.TraceData(TraceEventType.Error, (int)eventId, e);
			}
		}
	}
}
