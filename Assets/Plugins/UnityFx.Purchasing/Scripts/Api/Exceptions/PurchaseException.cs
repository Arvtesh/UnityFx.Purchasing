// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic purchase exception.
	/// </summary>
	[Serializable]
	public class PurchaseException : StoreException
	{
		#region data

		private readonly Product _product;
		private readonly PurchaseFailureReason _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets purchase error code.
		/// </summary>
		public Product Product
		{
			get
			{
				return _product;
			}
		}

		/// <summary>
		/// Gets purchase error code.
		/// </summary>
		public PurchaseFailureReason ErrorCode
		{
			get
			{
				return _reason;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseException"/> class.
		/// </summary>
		public PurchaseException(Product product, PurchaseFailureReason reason)
			: base(reason.ToString())
		{
			_product = product;
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseException"/> class.
		/// </summary>
		public PurchaseException(Product product, PurchaseFailureReason reason, StoreSpecificPurchaseErrorCode nativeError)
			: base(reason.ToString(), nativeError)
		{
			_product = product;
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseException"/> class.
		/// </summary>
		public PurchaseException(Product product, PurchaseFailureReason reason, Exception innerException)
			: base(reason.ToString(), innerException)
		{
			_product = product;
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseException"/> class.
		/// </summary>
		public PurchaseException(Product product, PurchaseFailureReason reason, StoreSpecificPurchaseErrorCode nativeError, Exception innerException)
			: base(reason.ToString(), nativeError, innerException)
		{
			_product = product;
			_reason = reason;
		}

		#endregion

		#region implementation
		#endregion
	}
}
