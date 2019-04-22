// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.FetchCompleted"/> and <see cref="IStoreService.InitializeCompleted"/>.
	/// </summary>
	public class FetchCompletedEventArgs : AsyncCompletedEventArgs
	{
		#region data

		private readonly int _id;
		private readonly InitializationFailureReason? _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets identifier of the operation.
		/// </summary>
		public int OperationId
		{
			get
			{
				return _id;
			}
		}

		/// <summary>
		/// Gets fetch failure reason.
		/// </summary>
		public InitializationFailureReason? ErrorCode
		{
			get
			{
				return _reason;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(int opId, object userState)
			: base(null, false, userState)
		{
			_id = opId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(InitializationFailureReason? failReason, Exception e, int opId, object userState)
			: base(e, false, userState)
		{
			_id = opId;
			_reason = failReason;
		}

		#endregion
	}
}
