// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	internal abstract class StoreOperationContainer
	{
		#region interface

		internal abstract StoreService Store { get; }
		internal abstract void AddOperation(StoreOperation op);
		internal abstract void ReleaseOperation(StoreOperation op);

		#endregion

		#region implementation
		#endregion
	}
}
