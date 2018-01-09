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

		private const string _resultSerializationName = "_result";
		private const string _reasonSerializationName = "_reason";

		#endregion

		#region interface

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
		public StorePurchaseException(PurchaseResult result, StorePurchaseError reason)
			: base(reason.ToString())
		{
			Result = result;
			Reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		public StorePurchaseException(PurchaseResult result, StorePurchaseError reason, Exception innerException)
			: base(reason.ToString(), innerException)
		{
			Result = result;
			Reason = reason;
		}

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="StorePurchaseException"/> class.
		/// </summary>
		private StorePurchaseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Result = info.GetValue(_resultSerializationName, typeof(PurchaseResult)) as PurchaseResult;
			Reason = (StorePurchaseError)info.GetValue(_reasonSerializationName, typeof(StorePurchaseError));
		}

		/// <inheritdoc/>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_resultSerializationName, Result, typeof(PurchaseResult));
			info.AddValue(_reasonSerializationName, Reason.ToString());
		}

		#endregion
	}
}
