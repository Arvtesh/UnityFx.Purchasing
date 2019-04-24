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
	public sealed class InitializeException : FetchException
	{
		#region data
		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="InitializeException"/> class.
		/// </summary>
		public InitializeException(InitializationFailureReason reason)
			: base(reason)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InitializeException"/> class.
		/// </summary>
		public InitializeException(InitializationFailureReason reason, Exception innerException)
			: base(reason, innerException)
		{
		}

		#endregion

		#region implementation
		#endregion
	}
}
