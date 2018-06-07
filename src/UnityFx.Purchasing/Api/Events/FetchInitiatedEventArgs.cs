// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.FetchInitiated"/> and <see cref="IStoreService.InitializeInitiated"/>.
	/// </summary>
	public class FetchInitiatedEventArgs : EventArgs
	{
		#region data

		private readonly int _id;
		private readonly object _userState;

		#endregion

		#region interface

		/// <summary>
		/// Gets identifier of the dismiss operation.
		/// </summary>
		public int OperationId => _id;

		/// <summary>
		/// Gets user-defined data assosisated with the operation.
		/// </summary>
		public object UserState => _userState;

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchInitiatedEventArgs"/> class.
		/// </summary>
		public FetchInitiatedEventArgs(int opId, object userState)
		{
			_id = opId;
			_userState = userState;
		}

		#endregion
	}
}
