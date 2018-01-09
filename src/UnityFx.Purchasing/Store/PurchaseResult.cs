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
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(Product product)
		{
			_product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			_product = transactionInfo.Product;
			_transaction = transactionInfo;
			_validationResult = validationResult;
		}

		#endregion
	}
}
