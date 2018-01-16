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
	public sealed class StoreFetchException : StoreException
	{
		#region data

		private const string _reasonSerializationName = "_reason";

		#endregion

		#region interface

		/// <summary>
		/// Returns initialization failure reason. Read only.
		/// </summary>
		public StoreFetchError Reason { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException(StoreFetchError reason)
			: base(reason.ToString())
		{
			Reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException(StoreFetchError reason, Exception innerException)
			: base(reason.ToString(), innerException)
		{
			Reason = reason;
		}

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		private StoreFetchException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Reason = (StoreFetchError)info.GetValue(_reasonSerializationName, typeof(StoreFetchError));
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(_reasonSerializationName, Reason.ToString());
		}

		#endregion
	}
}
