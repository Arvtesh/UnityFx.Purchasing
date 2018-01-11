// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A fetch operation.
	/// </summary>
	internal class FetchOperation : StoreOperation<object>
	{
		#region interface

		public FetchOperation(TraceSource console)
			: base(console, TraceEventId.Fetch, null, null)
		{
		}

		#endregion
	}
}
