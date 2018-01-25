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
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		/// <seealso cref="Product"/>
		string TransactionId { get; }

		/// <summary>
		/// Returns the identifier of the <see cref="Product"/>. Read only.
		/// </summary>
		/// <seealso cref="Product"/>
		string ProductId { get; }

		/// <summary>
		/// Returns the Unity product selected for purchase. Read only.
		/// </summary>
		/// <seealso cref="ProductId"/>
		/// <seealso cref="TransactionId"/>
		/// <seealso cref="Receipt"/>
		Product Product { get; }

		/// <summary>
		/// Returns native store-specific purchase receipt (differs from Unity receipt). Read only.
		/// </summary>
		/// <seealso cref="Product"/>
		string Receipt { get; }
	}
}
