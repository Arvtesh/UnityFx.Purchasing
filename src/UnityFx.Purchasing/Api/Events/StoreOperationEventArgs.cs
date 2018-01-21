// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for store events.
	/// </summary>
	public class StoreOperationEventArgs : EventArgs, IStoreOperationInfo
	{
		#region IStoreOperationInfo

		/// <inheritdoc/>
		public int OperationId { get; }

		/// <inheritdoc/>
		public object UserState { get; }

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreOperationEventArgs"/> class.
		/// </summary>
		public StoreOperationEventArgs(IStoreOperationInfo op)
		{
			OperationId = op.OperationId;
			UserState = op.UserState;
		}

		#endregion
	}
}
