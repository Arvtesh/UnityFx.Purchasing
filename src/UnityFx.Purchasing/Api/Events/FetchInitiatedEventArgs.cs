// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.FetchInitiated"/> and <see cref="IStoreService.InitializeInitiated"/>.
	/// </summary>
	public class FetchInitiatedEventArgs : StoreOperationEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FetchInitiatedEventArgs"/> class.
		/// </summary>
		public FetchInitiatedEventArgs(IStoreOperationInfo op)
			: base(op)
		{
		}
	}
}
