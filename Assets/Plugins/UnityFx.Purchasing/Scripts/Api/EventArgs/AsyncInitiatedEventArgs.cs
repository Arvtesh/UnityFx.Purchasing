// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for async operation initiate events.
	/// </summary>
	public class AsyncInitiatedEventArgs : EventArgs
	{
		#region data

		private readonly int _id;
		private readonly object _userState;

		#endregion

		#region interface

		/// <summary>
		/// Gets the unique identifier for the asynchronous task.
		/// </summary>
		public int OperationId
		{
			get
			{
				return _id;
			}
		}

		/// <summary>
		/// Gets user-defined data assosisated with the task.
		/// </summary>
		public object UserState
		{
			get
			{
				return _userState;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncInitiatedEventArgs"/> class.
		/// </summary>
		public AsyncInitiatedEventArgs(int opId, object userState)
		{
			_id = opId;
			_userState = userState;
		}

		#endregion
	}
}
