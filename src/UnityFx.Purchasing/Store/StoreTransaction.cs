// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store transaction information.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	[Serializable]
	public class StoreTransaction : ISerializable
	{
		#region data

		private const string _transactionSerializationName = "TransactionId";
		private const string _storeSerializationName = "StoreId";
		private const string _receiptSerializationName = "Receipt";
		private const string _restoredSerializationName = "IsRestored";

		#endregion

		#region interface

		/// <summary>
		/// Returns the Unity product selected for purchase. Read only.
		/// </summary>
		public Product Product { get; }

		/// <summary>
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		public string TransactionId { get; }

		/// <summary>
		/// Returns identifier of the target store. Read only.
		/// </summary>
		public string StoreId { get; }

		/// <summary>
		/// Returns native transaction receipt (differs from Unity receipt). Read only.
		/// </summary>
		public string Receipt { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsRestored { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(Product product, bool isRestored)
		{
			Product = product;
			TransactionId = product.transactionID;
			Receipt = product.GetNativeReceipt(out var storeId);
			StoreId = storeId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(Product product, string transactionId, string storeId, string receipt, bool isRestored)
		{
			Product = product;
			TransactionId = transactionId;
			StoreId = storeId;
			Receipt = receipt;
			IsRestored = isRestored;
		}

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		protected StoreTransaction(SerializationInfo info, StreamingContext context)
		{
			TransactionId = info.GetString(_transactionSerializationName);
			StoreId = info.GetString(_storeSerializationName);
			Receipt = info.GetString(_receiptSerializationName);
			IsRestored = info.GetBoolean(_restoredSerializationName);
		}

		/// <inheritdoc/>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(_transactionSerializationName, TransactionId);
			info.AddValue(_storeSerializationName, StoreId);
			info.AddValue(_receiptSerializationName, Receipt);
			info.AddValue(_restoredSerializationName, IsRestored);
		}

		#endregion
	}
}
