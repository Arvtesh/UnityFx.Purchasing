using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace UnityFx.Purchasing.Tests
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

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			switch (eventType)
			{
				case TraceEventType.Warning:
					UnityEngine.Debug.LogWarning("[<b>" + source + "</b>]: " + message);
					break;

				case TraceEventType.Error:
				case TraceEventType.Critical:
					UnityEngine.Debug.LogError("[<b>" + source + "</b>]: " + message);
					break;

				default:
					UnityEngine.Debug.Log("[<b>" + source + "</b>]: " + message);
					break;
			}
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			switch (eventType)
			{
				case TraceEventType.Warning:
					UnityEngine.Debug.LogWarning("[<b>" + source + "</b>]: " + data);
					break;

				case TraceEventType.Error:
				case TraceEventType.Critical:
					UnityEngine.Debug.LogError("[<b>" + source + "</b>]: " + data);
					break;

				default:
					UnityEngine.Debug.Log("[<b>" + source + "</b>]: " + data);
					break;
			}
		}
	}
}
