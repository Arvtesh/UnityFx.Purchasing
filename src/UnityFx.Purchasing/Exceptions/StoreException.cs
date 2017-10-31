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
	public class StoreException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException()
			: base("Purchase error")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		protected StoreException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
