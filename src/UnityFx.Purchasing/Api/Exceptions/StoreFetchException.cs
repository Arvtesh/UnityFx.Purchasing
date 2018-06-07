// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;

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

		private readonly StoreFetchError _reason = StoreFetchError.Unknown;

		#endregion

		#region interface

		/// <summary>
		/// Gets initialization failure reason.
		/// </summary>
		public StoreFetchError Reason => _reason;

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
		public StoreFetchException(StoreFetchError reason, int opId, object userState)
			: base(reason.ToString(), opId, userState)
		{
			_reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreFetchException"/> class.
		/// </summary>
		public StoreFetchException(StoreFetchError reason, Exception innerException, int opId, object userState)
			: base(reason.ToString(), innerException, opId, userState)
		{
			_reason = reason;
		}

		#endregion

		#region ISerializable

		private StoreFetchException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_reason = (StoreFetchError)info.GetValue(_reasonSerializationName, typeof(StoreFetchError));
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_reasonSerializationName, _reason);
		}

		#endregion
	}
}
