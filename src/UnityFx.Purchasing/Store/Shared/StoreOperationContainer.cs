// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	internal abstract class StoreOperationContainer
	{
		#region interface

		public abstract StoreService Store { get; }
		public abstract void ReleaseOperation(IAsyncOperation op);

		#endregion

		#region implementation
		#endregion
	}
}
