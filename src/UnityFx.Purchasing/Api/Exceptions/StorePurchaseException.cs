// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic purchase exception.
	/// </summary>
	[Serializable]
	public sealed class StorePurchaseException : StoreException, IPurchaseResult
	{
		#region data

		private const string _productIdSerializationName = "_productId";
		private const string _transactionSerializationName = "_transactionId";
		private const string _receiptSerializationName = "_receipt";
		private const string _restoredSerializationName = "_restored";
		private const string _reasonSerializationName = "_reason";

		private readonly string _productId;
		private readonly Product _product;
		private readonly string _transactionId;
		private readonly string _receipt;
		private readonly PurchaseValidationResult _validationResult;
		private readonly bool _restored;
		private readonly StorePurchaseError _reason = StorePurchaseError.Unknown;

		#endregion

		#region interface

		/// <summary>
		/// Gets the purchase error identifier.
		/// </summary>
		public StorePurchaseError Reason => _reason;

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(IPurchaseResult result, StorePurchaseError reason)
			: base(GetMessage(result.ProductId, reason), result)
		{
			_productId = result.ProductId;
			_product = result.Product;
			_transactionId = result.TransactionId;
			_receipt = result.Receipt;
			_validationResult = result.ValidationResult;
			_restored = result.Restored;
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(IPurchaseResult result, StorePurchaseError reason, Exception innerException)
			: base(GetMessage(result.ProductId, reason), result, innerException)
		{
			_productId = result.ProductId;
			_product = result.Product;
			_transactionId = result.TransactionId;
			_receipt = result.Receipt;
			_validationResult = result.ValidationResult;
			_restored = result.Restored;
			_reason = reason;
		}

		#endregion

		#region Exception

		/// <inheritdoc/>
		public override string Message
		{
			get
			{
				var s = base.Message;

				if (_transactionId != null)
				{
					s += " TransactionID: " + _transactionId;

					if (_restored)
					{
						s += ", auto-restored.";
					}
					else
					{
						s += '.';
					}
				}

				return s;
			}
		}

		#endregion

		#region IPurchaseResult

		/// <inheritdoc/>
		public PurchaseValidationResult ValidationResult => _validationResult;

		#endregion

		#region IStoreTransaction

		/// <inheritdoc/>
		public string ProductId => _productId;

		/// <inheritdoc/>
		public Product Product => _product;

		/// <inheritdoc/>
		public string TransactionId => _transactionId;

		/// <inheritdoc/>
		public string Receipt => _receipt;

		/// <inheritdoc/>
		public bool Restored => _restored;

		#endregion

		#region ISerializable

		private StorePurchaseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_productId = info.GetString(_productIdSerializationName);
			_transactionId = info.GetString(_transactionSerializationName);
			_receipt = info.GetString(_receiptSerializationName);
			_restored = info.GetBoolean(_restoredSerializationName);
			_reason = (StorePurchaseError)info.GetValue(_reasonSerializationName, typeof(StorePurchaseError));
		}

		/// <inheritdoc/>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_productIdSerializationName, _productId);
			info.AddValue(_transactionSerializationName, _transactionId);
			info.AddValue(_receiptSerializationName, _receipt);
			info.AddValue(_restoredSerializationName, _restored);
			info.AddValue(_reasonSerializationName, _reason);
		}

		#endregion

		#region implementation

		private static string GetMessage(string productId, StorePurchaseError reason)
		{
			return $"In-app purchase failed for product {productId} ({reason.ToString()}).";
		}

		#endregion
	}
}
