// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.Serialization;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Enumerates purchase validation results.
	/// </summary>
	public enum PurchaseValidationStatus
	{
		/// <summary>
		/// Validation succeeded.
		/// </summary>
		Ok,

		/// <summary>
		/// Validation failed: the purchase receipt is invalid.
		/// </summary>
		Failure,

		/// <summary>
		/// Result is not available. The purchase will not be commited ans has to be re-validated. For example transport error while executing verification request.
		/// </summary>
		NotAvailable,
	}

	/// <summary>
	/// A purchase validation result.
	/// </summary>
	[Serializable]
	public class PurchaseValidationResult : ISerializable
	{
		#region data

		private const string _statusSerializationName = "Status";

		#endregion

		#region interface

		/// <summary>
		/// Returns the validation status. Read only.
		/// </summary>
		public PurchaseValidationStatus Status { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(PurchaseValidationStatus status)
		{
			Status = status;
		}

		#endregion

		#region ISerializable

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		protected PurchaseValidationResult(SerializationInfo info, StreamingContext context)
		{
			Status = (PurchaseValidationStatus)info.GetValue(_statusSerializationName, typeof(PurchaseValidationStatus));
		}

		/// <inheritdoc/>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(_statusSerializationName, Status);
		}

		#endregion
	}
}
