// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for initialize/fetch events.
	/// </summary>
	public class FetchInitiatedEventArgs : StoreOperationEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FetchInitiatedEventArgs"/> class.
		/// </summary>
		public FetchInitiatedEventArgs(IStoreOperation op)
			: base(op)
		{
		}
	}
}
