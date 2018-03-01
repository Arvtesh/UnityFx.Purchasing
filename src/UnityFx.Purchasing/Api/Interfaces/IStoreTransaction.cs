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
		/// Gets Unity product selected for purchase.
		/// </summary>
		/// <value><c>Unity3d</c> product.</value>
		/// <seealso cref="ProductId"/>
		/// <seealso cref="TransactionId"/>
		/// <seealso cref="Receipt"/>
		Product Product { get; }

		/// <summary>
		/// Gets a unique identifier of the <see cref="Product"/>.
		/// </summary>
		/// <value>Unique product identifier.</value>
		/// <seealso cref="Product"/>
		string ProductId { get; }

		/// <summary>
		/// Gets identifier of the transaction.
		/// </summary>
		/// <value>A unique identifier for the <see cref="Product"/>'s transaction.</value>
		/// <seealso cref="Product"/>
		string TransactionId { get; }

		/// <summary>
		/// Gets native store-specific purchase receipt (differs from Unity receipt).
		/// </summary>
		/// <value>The purchase receipt.</value>
		/// <seealso cref="Product"/>
		string Receipt { get; }

		/// <summary>
		/// Gets a value indicating whether the purchase was auto-restored.
		/// </summary>
		/// <value><see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise.</value>
		bool Restored { get; }
	}
}
