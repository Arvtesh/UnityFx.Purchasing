// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace UnityFx.Purchasing.Examples
{
	public class UnityTraceListener : TraceListener
	{
		public UnityTraceListener()
			: base("UnityTraceListener")
		{
		}

		public override void Write(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		public override void WriteLine(string message)
		{
			UnityEngine.Debug.Log(message);
		}

		[DebuggerStepThrough]
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			var str = $"[<b>{source}</b> {id}]: <color=silver>{message}</color>";

			switch (eventType)
			{
				case TraceEventType.Verbose:
					UnityEngine.Debug.Log($"[<b>{source}</b> {id}]: {message}");
					break;

				case TraceEventType.Warning:
					UnityEngine.Debug.LogWarning(str);
					break;

				case TraceEventType.Error:
				case TraceEventType.Critical:
					UnityEngine.Debug.LogError(str);
					break;

				default:
					UnityEngine.Debug.Log(str);
					break;
			}
		}

		[DebuggerStepThrough]
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			var str = $"[<b>{source}</b> {id}]: {data}";

			switch (eventType)
			{
				case TraceEventType.Warning:
					UnityEngine.Debug.LogWarning(str);
					break;

				case TraceEventType.Error:
				case TraceEventType.Critical:
					UnityEngine.Debug.LogError(str);
					break;

				default:
					UnityEngine.Debug.Log(str);
					break;
			}
		}
	}
}
