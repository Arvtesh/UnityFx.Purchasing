// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for store events.
	/// </summary>
	public class StoreOperationEventArgs : EventArgs
	{
		/// <summary>
		/// Returns identifier of the corresponding operation. Read only.
		/// </summary>
		public int OperationId { get; }

		/// <summary>
		/// Returns user-defined data assosiated with the corresponding operation (if any). Read only.
		/// </summary>
		public object UserData { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreOperationEventArgs"/> class.
		/// </summary>
		public StoreOperationEventArgs(IStoreOperation op)
		{
			OperationId = op.Id;
			UserData = op.AsyncState;
		}
	}
}
