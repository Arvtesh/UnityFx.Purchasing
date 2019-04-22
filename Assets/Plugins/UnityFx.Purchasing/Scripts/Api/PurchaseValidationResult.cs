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
		/// Validation result is not available.
		/// </summary>
		NotAvailable,

		/// <summary>
		/// Validation succeeded.
		/// </summary>
		Ok,

		/// <summary>
		/// Validation failed: the purchase receipt is invalid.
		/// </summary>
		Failed
	}

	/// <summary>
	/// A purchase validation result.
	/// </summary>
	[Serializable]
	public struct PurchaseValidationResult
	{
		#region data

		private readonly PurchaseValidationStatus _status;

#if UNITY_IOS
		private readonly AppleReceipt _receipt;
#elif UNITY_ANDROID
		private readonly GooglePlayReceipt _receipt;
#endif

		#endregion

		#region interface

		/// <summary>
		/// Gets the validation status.
		/// </summary>
		/// <value>Identifier of the validation status.</value>
		public PurchaseValidationStatus Status
		{
			get
			{
				return _status;
			}
		}

#if UNITY_IOS

		public AppleReceipt Receipt
		{
			get
			{
				return _receipt;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(AppleReceipt receipt)
		{
			_status = PurchaseValidationStatus.Ok;
			_receipt = receipt;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(PurchaseValidationStatus status, AppleReceipt receipt)
		{
			_status = status;
			_receipt = receipt;
		}

#elif UNITY_ANDROID

		public GooglePlayReceipt Receipt
		{
			get
			{
				return _receipt;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(GooglePlayReceipt receipt)
		{
			_status = PurchaseValidationStatus.Ok;
			_receipt = receipt;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseValidationResult"/> class.
		/// </summary>
		public PurchaseValidationResult(PurchaseValidationStatus status, GooglePlayReceipt receipt)
		{
			_status = status;
			_receipt = receipt;
		}

#endif

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
