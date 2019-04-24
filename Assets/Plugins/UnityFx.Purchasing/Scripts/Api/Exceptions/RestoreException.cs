// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A generic restore exception.
	/// </summary>
	[Serializable]
	public class RestoreException : StoreException
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="RestoreException"/> class.
		/// </summary>
		public RestoreException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RestoreException"/> class.
		/// </summary>
		public RestoreException(StoreSpecificPurchaseErrorCode nativeError)
			: base(nativeError.ToString())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RestoreException"/> class.
		/// </summary>
		public RestoreException(StoreSpecificPurchaseErrorCode nativeError, Exception innerException)
			: base(nativeError.ToString(), innerException)
		{
		}

		#endregion

		#region implementation
		#endregion
	}
}
