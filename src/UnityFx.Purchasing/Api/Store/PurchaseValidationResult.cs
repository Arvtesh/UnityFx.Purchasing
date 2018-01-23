// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

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
		/// Validation is not implemented.
		/// </summary>
		Suppressed,

		/// <summary>
		/// Result is not available. The purchase will not be commited and has to be re-validated. For example transport error while executing verification request.
		/// </summary>
		NotAvailable,
	}

	/// <summary>
	/// A purchase validation result.
	/// </summary>
	[Serializable]
	public class PurchaseValidationResult
	{
		#region data

		private readonly PurchaseValidationStatus _status;

		private static PurchaseValidationResult _success;
		private static PurchaseValidationResult _failure;
		private static PurchaseValidationResult _notAvailable;
		private static PurchaseValidationResult _suppressed;

		#endregion

		#region interface

		/// <summary>
		/// Returns a <see cref="PurchaseValidationResult"/> instance with status <see cref="PurchaseValidationStatus.Ok"/>. Read only.
		/// </summary>
		public static PurchaseValidationResult Success
		{
			get
			{
				if (_success == null)
				{
					_success = new PurchaseValidationResult(PurchaseValidationStatus.Ok);
				}

				return _success;
			}
		}

		/// <summary>
		/// Returns a <see cref="PurchaseValidationResult"/> instance with status <see cref="PurchaseValidationStatus.Failure"/>. Read only.
		/// </summary>
		public static PurchaseValidationResult Failure
		{
			get
			{
				if (_failure == null)
				{
					_failure = new PurchaseValidationResult(PurchaseValidationStatus.Failure);
				}

				return _failure;
			}
		}

		/// <summary>
		/// Returns a <see cref="PurchaseValidationResult"/> instance with status <see cref="PurchaseValidationStatus.NotAvailable"/>. Read only.
		/// </summary>
		public static PurchaseValidationResult NotAvailable
		{
			get
			{
				if (_notAvailable == null)
				{
					_notAvailable = new PurchaseValidationResult(PurchaseValidationStatus.NotAvailable);
				}

				return _notAvailable;
			}
		}

		/// <summary>
		/// Returns a <see cref="PurchaseValidationResult"/> instance with status <see cref="PurchaseValidationStatus.Suppressed"/>. Read only.
		/// </summary>
		public static PurchaseValidationResult Suppressed
		{
			get
			{
				if (_suppressed == null)
				{
					_suppressed = new PurchaseValidationResult(PurchaseValidationStatus.Suppressed);
				}

				return _suppressed;
			}
		}

		/// <summary>
		/// Returns the validation status. Read only.
		/// </summary>
		public PurchaseValidationStatus Status => _status;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(PurchaseValidationStatus status)
		{
			_status = status;
		}

		#endregion
	}
}
