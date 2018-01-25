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
	public class StoreException : Exception, IStoreOperationInfo
	{
		#region data

		private const string _idSerializationName = "_id";

		private readonly int _id;
		private readonly object _userState;

		#endregion

		#region interface

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
		public StoreException(string message, IStoreOperationInfo op)
			: base(message)
		{
			_id = op.OperationId;
			_userState = op.UserState;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		public StoreException(string message, IStoreOperationInfo op, Exception innerException)
			: base(message, innerException)
		{
			_id = op.OperationId;
			_userState = op.UserState;
		}

		#endregion

		#region IStoreOperationInfo

		/// <inheritdoc/>
		public int OperationId => _id;

		/// <inheritdoc/>
		public object UserState => _userState;

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreException"/> class.
		/// </summary>
		protected StoreException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_id = info.GetInt32(_idSerializationName);
		}

		/// <inheritdoc/>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(_idSerializationName, _id);
		}

		#endregion
	}
}
