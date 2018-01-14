// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for fetch-related events.
	/// </summary>
	public class StoreFetchEventArgs : EventArgs
	{
		/// <summary>
		/// Returns <see langword="true"/> if this event is caused by a <see cref="IStoreService.Fetch"/> call;
		/// <see langword="false"/> if <see cref="IStoreService.Initialize"/> was called. Read only.
		/// </summary>
		public bool IsFetch { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchEventArgs"/> class.
		/// </summary>
		public StoreFetchEventArgs(bool fetch)
		{
			IsFetch = fetch;
		}
	}
}
