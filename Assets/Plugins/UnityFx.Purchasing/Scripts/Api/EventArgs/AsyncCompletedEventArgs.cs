// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for async completion initiate events.
	/// </summary>
	public class AsyncCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{
		#region data

		private readonly int _id;

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
		/// Gets a value indicating whether the operation completed successfully.
		/// </summary>
		public bool IsCompletedSuccessfully
		{
			get
			{
				return Error == null && !Cancelled;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the operation failed.
		/// </summary>
		public bool IsFailed
		{
			get
			{
				return Error != null || Cancelled;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletedEventArgs"/> class.
		/// </summary>
		public AsyncCompletedEventArgs(int opId, object userState)
			: base(null, false, userState)
		{
			_id = opId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletedEventArgs"/> class.
		/// </summary>
		public AsyncCompletedEventArgs(int opId, object userState, Exception error, bool cancelled)
			: base(error, cancelled, userState)
		{
			_id = opId;
		}

		#endregion
	}
}
