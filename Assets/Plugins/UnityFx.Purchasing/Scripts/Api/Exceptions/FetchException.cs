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
	public class FetchException : StoreException
	{
		#region data

		private readonly InitializationFailureReason _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets initialization failure reason.
		/// </summary>
		public InitializationFailureReason ErrorCode
		{
			get
			{
				return _reason;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchException"/> class.
		/// </summary>
		public FetchException(InitializationFailureReason reason)
			: base(reason.ToString())
		{
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchException"/> class.
		/// </summary>
		public FetchException(InitializationFailureReason reason, StoreSpecificPurchaseErrorCode nativeError)
			: base(reason.ToString(), nativeError)
		{
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchException"/> class.
		/// </summary>
		public FetchException(InitializationFailureReason reason, Exception innerException)
			: base(reason.ToString(), innerException)
		{
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchException"/> class.
		/// </summary>
		public FetchException(InitializationFailureReason reason, StoreSpecificPurchaseErrorCode nativeError, Exception innerException)
			: base(reason.ToString(), nativeError, innerException)
		{
			_reason = reason;
		}

		#endregion

		#region implementation
		#endregion
	}
}
