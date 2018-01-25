// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a failed store purchase.
	/// </summary>
	public struct FailedPurchaseResult : IPurchaseResult
	{
		#region data

		private readonly IPurchaseResult _result;
		private readonly StorePurchaseError _reason;
		private readonly Exception _exception;

		#endregion

		#region interface

		/// <summary>
		/// Returns purchase failure reason. Read only.
		/// </summary>
		public StorePurchaseError ErrorId => _reason;

		/// <summary>
		/// Returns an exception which occurred during the operation. Read only.
		/// </summary>
		public Exception Error => _exception;

		/// <summary>
		/// Returns a value indicating whether an asynchronous operation has been canceled. Read only.
		/// </summary>
		public bool Cancelled => _reason == StorePurchaseError.UserCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="FailedPurchaseResult"/> struct.
		/// </summary>
		internal FailedPurchaseResult(IPurchaseResult result, StorePurchaseError failReason, Exception e)
		{
			_result = result;
			_reason = failReason;
			_exception = e;
		}

		#endregion

		#region IPurchaseResult

		/// <inheritdoc/>
		public PurchaseValidationResult ValidationResult => _result.ValidationResult;

		/// <inheritdoc/>
		public bool Restored => _result.Restored;

		#endregion

		#region IStoreTransaction

		/// <inheritdoc/>
		public string ProductId => _result.ProductId;

		/// <inheritdoc/>
		public Product Product => _result.Product;

		/// <inheritdoc/>
		public string TransactionId => _result.TransactionId;

		/// <inheritdoc/>
		public string Receipt => _result.Receipt;

		#endregion

		#region IStoreOperationInfo

		/// <inheritdoc/>
		public int OperationId => _result.OperationId;

		/// <inheritdoc/>
		public object UserState => _result.UserState;

		#endregion
	}
}
