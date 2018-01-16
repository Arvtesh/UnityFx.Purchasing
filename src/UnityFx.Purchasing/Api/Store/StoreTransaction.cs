// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store transaction information.
	/// </summary>
	/// <seealso cref="IStoreService"/>
	[Serializable]
	public class StoreTransaction
	{
		#region data

		[NonSerialized]
		private Product _product;

		private string _transactionId;
		private string _storeId;
		private string _receipt;
		private bool _restored;

		#endregion

		#region interface

		/// <summary>
		/// Returns the Unity product selected for purchase. Read only.
		/// </summary>
		public Product Product => _product;

		/// <summary>
		/// Returns identifier of the transaction. Read only.
		/// </summary>
		public string TransactionId => _transactionId;

		/// <summary>
		/// Returns identifier of the target store. Read only.
		/// </summary>
		public string StoreId => _storeId;

		/// <summary>
		/// Returns native transaction receipt (differs from Unity receipt). Read only.
		/// </summary>
		public string Receipt => _receipt;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsRestored => _restored;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(Product product, bool isRestored)
		{
			_product = product;
			_transactionId = product.transactionID;
			_receipt = product.GetNativeReceipt(out var storeId);
			_storeId = storeId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreTransaction"/> class.
		/// </summary>
		public StoreTransaction(Product product, string transactionId, string storeId, string receipt, bool isRestored)
		{
			_product = product;
			_transactionId = transactionId;
			_storeId = storeId;
			_receipt = receipt;
			_restored = isRestored;
		}

		#endregion
	}
}
