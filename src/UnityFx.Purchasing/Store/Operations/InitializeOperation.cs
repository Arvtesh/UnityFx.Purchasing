﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Represents an initialize operation.
	/// </summary>
	internal class InitializeOperation : StoreOperation<object>
	{
		#region interface

		public InitializeOperation(TraceSource console)
			: base(console, StoreService.TraceEventId.Initialize, null, null)
		{
		}

		#endregion
	}
}
