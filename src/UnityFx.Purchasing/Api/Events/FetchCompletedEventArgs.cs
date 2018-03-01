// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.FetchCompleted"/> and <see cref="IStoreService.InitializeCompleted"/>.
	/// </summary>
	public class FetchCompletedEventArgs : AsyncCompletedEventArgs, IStoreOperationInfo
	{
		#region data

		private readonly IStoreOperationInfo _result;
		private readonly StoreFetchError _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets fetch failure reason.
		/// </summary>
		public StoreFetchError ErrorReason => _reason;

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(IStoreOperationInfo op)
			: base(null, false, op.UserState)
		{
			_result = op;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(IStoreOperationInfo op, StoreFetchError failReason, Exception e)
			: base(e, false, op.UserState)
		{
			_result = op;
			_reason = failReason;
		}

		#endregion

		#region IStoreOperationInfo

		/// <inheritdoc/>
		public int OperationId => _result.OperationId;

		#endregion
	}
}
