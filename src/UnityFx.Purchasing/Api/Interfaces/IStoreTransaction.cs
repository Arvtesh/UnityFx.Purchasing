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
		/// <value><c>Unity3d</c> product.</value>
		/// <seealso cref="ProductId"/>
		/// <seealso cref="TransactionId"/>
		/// <seealso cref="Receipt"/>
		Product Product { get; }

		/// <summary>
		/// Returns a unique identifier of the <see cref="Product"/>. Read only.
		/// </summary>
		/// <value>Unique product identifier.</value>
		/// <seealso cref="Product"/>
		string ProductId { get; }

		/// <summary>
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		/// <value>A unique identifier for the <see cref="Product"/>'s transaction.</value>
		/// <seealso cref="Product"/>
		string TransactionId { get; }

		/// <summary>
		/// Returns native store-specific purchase receipt (differs from Unity receipt). Read only.
		/// </summary>
		/// <value>The purchase receipt.</value>
		/// <seealso cref="Product"/>
		string Receipt { get; }
	}
}
