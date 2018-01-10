// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.StoreFetchFailed"/>.
	/// </summary>
	public class StoreFetchFailedEventArgs : StoreFetchEventArgs
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
		/// Initializes a new instance of the <see cref="StoreFetchFailedEventArgs"/> class.
		/// </summary>
		public StoreFetchFailedEventArgs(bool fetch, StoreFetchError reason, Exception e)
			: base(fetch)
		{
			Reason = reason;
			Exception = e;
		}
	}
}
