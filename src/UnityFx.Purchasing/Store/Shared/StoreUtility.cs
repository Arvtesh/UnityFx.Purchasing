// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

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
		public static void TraceOperationBegin(TraceSource console, TraceEventId eventId, string comment, string args)
		{
			var s = eventId.ToString();

			if (!string.IsNullOrEmpty(comment))
			{
				s += " (" + comment + ')';
			}

			if (!string.IsNullOrEmpty(args))
			{
				s += ": " + args;
			}

			console.TraceEvent(TraceEventType.Start, (int)eventId, s);
		}

		public static void TraceOperationComplete(TraceSource console, TraceEventId eventId, string args)
		{
			if (string.IsNullOrEmpty(args))
			{
				console.TraceEvent(TraceEventType.Stop, (int)eventId, eventId.ToString() + " completed");
			}
			else
			{
				console.TraceEvent(TraceEventType.Stop, (int)eventId, eventId.ToString() + " completed: " + args);
			}
		}

		public static void TraceOperationFailed(TraceSource console, TraceEventId eventId, string args)
		{
			if (string.IsNullOrEmpty(args))
			{
				console.TraceEvent(TraceEventType.Stop, (int)eventId, eventId.ToString() + " failed");
			}
			else
			{
				console.TraceEvent(TraceEventType.Stop, (int)eventId, eventId.ToString() + " failed: " + args);
			}
		}
	}
}
