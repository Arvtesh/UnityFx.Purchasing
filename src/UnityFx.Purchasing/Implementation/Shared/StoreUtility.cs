// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	internal enum AsyncPatternType
	{
		Default,
		Eap,
		Apm,
		Tap
	}

	internal enum StoreOperationType
	{
		Initialize,
		Fetch,
		Purchase
	}

	internal static class StoreUtility
	{
		internal static int GetOperationId(int n, StoreOperationType opType, AsyncPatternType asyncPattern)
		{
			return (n << 4) | ((int)asyncPattern << 2) | (int)opType;
		}

		internal static string GetOperationType(int id)
		{
			if ((id & (int)StoreOperationType.Purchase) != 0)
			{
				return StoreOperationType.Purchase.ToString();
			}
			else if ((id & (int)StoreOperationType.Fetch) != 0)
			{
				return StoreOperationType.Fetch.ToString();
			}
			else if ((id & (int)StoreOperationType.Initialize) != 0)
			{
				return StoreOperationType.Initialize.ToString();
			}

			return "UnknownOperation";
		}

		internal static void TraceException(this TraceSource traceSource, StoreOperationType opId, Exception e, TraceEventType eventType = TraceEventType.Error)
		{
			if (e != null)
			{
				traceSource.TraceData(eventType, (int)opId, e);
			}
		}
	}
}
