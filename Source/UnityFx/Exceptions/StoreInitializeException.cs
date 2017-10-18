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
	public sealed class StoreInitializeException : StoreException
	{
		/// <summary>
		/// Returns initialization failure reason. Read only.
		/// </summary>
		public InitializationFailureReason Reason { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreInitializeException"/> class.
		/// </summary>
		public StoreInitializeException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreInitializeException"/> class.
		/// </summary>
		public StoreInitializeException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreInitializeException"/> class.
		/// </summary>
		public StoreInitializeException(InitializationFailureReason reason)
			: base(reason.ToString())
		{
			Reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreInitializeException"/> class.
		/// </summary>
		public StoreInitializeException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreInitializeException"/> class.
		/// </summary>
		private StoreInitializeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}
