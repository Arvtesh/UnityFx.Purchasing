// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store transaction information.
	/// </summary>
	public interface IStoreTransaction
	{
		/// <summary>
		/// Returns the Unity product selected for purchase. Read only.
		/// </summary>
		Product Product { get; }

		/// <summary>
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		string TransactionId { get; }

		/// <summary>
		/// Returns native transaction receipt (differs from Unity receipt). Read only.
		/// </summary>
		string Receipt { get; }
	}
}
