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
	public abstract class StoreException : Exception
	{
		#region data

		private readonly StoreSpecificPurchaseErrorCode _nativeErrorCode;

		#endregion

		#region interface

		/// <summary>
		/// Gets native error code (if available).
		/// </summary>
		public StoreSpecificPurchaseErrorCode NativeErrorCode
		{
			get
			{
				return _nativeErrorCode;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException()
		{
			_nativeErrorCode = StoreSpecificPurchaseErrorCode.Unknown;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message)
			: base(message)
		{
			_nativeErrorCode = StoreSpecificPurchaseErrorCode.Unknown;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message, StoreSpecificPurchaseErrorCode errorCode)
			: base(message)
		{
			_nativeErrorCode = errorCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message, Exception innerException)
			: base(message, innerException)
		{
			_nativeErrorCode = StoreSpecificPurchaseErrorCode.Unknown;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message, StoreSpecificPurchaseErrorCode errorCode, Exception innerException)
			: base(message, innerException)
		{
			_nativeErrorCode = errorCode;
		}

		#endregion

		#region implementation
		#endregion
	}
}
