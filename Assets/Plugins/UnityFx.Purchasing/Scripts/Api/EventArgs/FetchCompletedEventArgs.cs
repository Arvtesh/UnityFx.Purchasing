// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.FetchCompleted"/> and <see cref="IStoreService.InitializeCompleted"/>.
	/// </summary>
	public class FetchCompletedEventArgs : AsyncCompletedEventArgs
	{
		#region data

		private readonly InitializationFailureReason? _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets initiate/fetch failure reason.
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
			: base(opId, userState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(int opId, object userState, FetchException e)
			: base(opId, userState, e, false)
		{
			_reason = e.ErrorCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(int opId, object userState, Exception e)
			: base(opId, userState, e, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(int opId, object userState, Exception e, InitializationFailureReason failReason)
			: base(opId, userState, e, false)
		{
			_reason = failReason;
		}

		#endregion
	}
}
