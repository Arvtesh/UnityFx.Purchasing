// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic purchase exception.
	/// </summary>
	[Serializable]
	public sealed class StorePurchaseException : StoreException
	{
		#region data

		private const string _productIdSerializationName = "_productId";
		private const string _resultSerializationName = "_result";
		private const string _reasonSerializationName = "_reason";

		#endregion

		#region interface

		/// <summary>
		/// Returns the product identifier. Read only.
		/// </summary>
		public string ProductId { get; }

		/// <summary>
		/// Returns the purchase result. Read only.
		/// </summary>
		public PurchaseResult Result { get; }

		/// <summary>
		/// Returns the purchase error identifier. Read only.
		/// </summary>
		public StorePurchaseError Reason { get; }

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
		public StorePurchaseException(FailedPurchaseResult result)
			: base(GetMessage(result.ProductId, result.Reason), result.Exception)
		{
			ProductId = result.ProductId;
			Result = result;
			Reason = result.Reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(string productId, PurchaseResult result, StorePurchaseError reason)
			: base(GetMessage(productId, reason))
		{
			ProductId = productId;
			Result = result;
			Reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(string productId, PurchaseResult result, StorePurchaseError reason, Exception innerException)
			: base(GetMessage(productId, reason), innerException)
		{
			ProductId = productId;
			Result = result;
			Reason = reason;
		}

		#endregion

		#region Exception

		/// <inheritdoc/>
		public override string Message
		{
			get
			{
				var s = base.Message;
				var transactionInfo = Result.TransactionInfo;

				if (transactionInfo != null)
				{
					s += " TransactionID: " + transactionInfo.TransactionId;

					if (transactionInfo.IsRestored)
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

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		private StorePurchaseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			ProductId = info.GetString(_productIdSerializationName);
			Result = info.GetValue(_resultSerializationName, typeof(PurchaseResult)) as PurchaseResult;
			Reason = (StorePurchaseError)info.GetValue(_reasonSerializationName, typeof(StorePurchaseError));
		}

		/// <inheritdoc/>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_productIdSerializationName, ProductId);
			info.AddValue(_resultSerializationName, Result, typeof(PurchaseResult));
			info.AddValue(_reasonSerializationName, Reason.ToString());
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
