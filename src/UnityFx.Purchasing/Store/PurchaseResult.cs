// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A store purchase result.
	/// </summary>
	[Serializable]
	public class PurchaseResult : ISerializable
	{
		#region data

		private const string _transactionSerializationName = "TransactionInfo";
		private const string _validationResultSerializationName = "ValidationResult";

		#endregion

		#region interface

		/// <summary>
		/// Returns the purchased product (or <see langword="null"/>). Read only.
		/// </summary>
		public Product Product { get; }

		/// <summary>
		/// Returns the transaction info. Read only.
		/// </summary>
		public StoreTransaction TransactionInfo { get; }

		/// <summary>
		/// Returns product validation result (<see langword="null"/> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(Product product)
		{
			Product = product;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(StoreTransaction transactionInfo, PurchaseValidationResult validationResult)
		{
			Product = transactionInfo.Product;
			TransactionInfo = transactionInfo;
			ValidationResult = validationResult;
		}

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		protected PurchaseResult(SerializationInfo info, StreamingContext context)
		{
			TransactionInfo = info.GetValue(_transactionSerializationName, typeof(StoreTransaction)) as StoreTransaction;
			ValidationResult = info.GetValue(_validationResultSerializationName, typeof(PurchaseValidationResult)) as PurchaseValidationResult;
		}

		/// <inheritdoc/>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(_transactionSerializationName, TransactionInfo, typeof(StoreTransaction));
			info.AddValue(_validationResultSerializationName, ValidationResult, typeof(PurchaseValidationResult));
		}

		#endregion
	}
}
