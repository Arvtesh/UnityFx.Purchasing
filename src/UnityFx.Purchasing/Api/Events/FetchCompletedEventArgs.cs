// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreEvents.FetchCompleted"/>.
	/// </summary>
	public class FetchCompletedEventArgs : StoreOperationEventArgs
	{
		/// <summary>
		/// Returns initialization failure reason. Read only.
		/// </summary>
		public StoreFetchError Reason { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the corresponding operation has completed successfully; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsSucceeded { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the corresponding operation has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsFaulted { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(IStoreOperation op)
			: base(op)
		{
			IsSucceeded = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchCompletedEventArgs"/> class.
		/// </summary>
		public FetchCompletedEventArgs(IStoreOperation op, StoreFetchError reason, Exception e)
			: base(op)
		{
			Reason = reason;
			Exception = e;
			IsFaulted = true;
		}
	}
}
