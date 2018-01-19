// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result.
	/// </summary>
	[Serializable]
	public class PurchaseResult
	{
		#region data

		[NonSerialized]
		private Product _product;

		private StoreTransaction _transaction;
		private PurchaseValidationResult _validationResult;
		private string _productId;
		private bool _restored;

		#endregion

		#region interface

		/// <summary>
		/// Returns the purchased product (or <see langword="null"/>). Read only.
		/// </summary>
		public Product Product => _product;

		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		public StoreTransaction TransactionInfo => _transaction;

		/// <summary>
		/// Returns product validation result (<see langword="null"/> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult => _validationResult;

		/// <summary>
		/// Returns the product identifier. Read only.
		/// </summary>
		public string ProductId => _productId;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase was auto-restored; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsRestored => _restored;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(Product product, bool restored)
		{
			_product = product;
			_productId = product.definition.id;
			_restored = restored;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		protected PurchaseResult(string productId, Product product, bool restored)
		{
			_product = product;
			_productId = productId;
			_restored = restored;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(StoreTransaction transactionInfo, PurchaseValidationResult validationResult, bool restored)
		{
			_product = transactionInfo.Product;
			_productId = transactionInfo.ProductId;
			_transaction = transactionInfo;
			_validationResult = validationResult;
			_restored = restored;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		protected PurchaseResult(string productId, StoreTransaction transactionInfo, PurchaseValidationResult validationResult, bool restored)
		{
			_product = transactionInfo.Product;
			_productId = productId;
			_transaction = transactionInfo;
			_validationResult = validationResult;
			_restored = restored;
		}

		#endregion
	}
}
