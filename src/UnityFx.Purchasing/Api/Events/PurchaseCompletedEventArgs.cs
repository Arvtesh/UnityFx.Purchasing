﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseCompleted"/>.
	/// </summary>
	public class PurchaseCompletedEventArgs : AsyncCompletedEventArgs, IPurchaseResult
	{
		#region data

		private readonly IPurchaseResult _result;
		private readonly StorePurchaseError _reason;

		#endregion

		#region interface

		/// <summary>
		/// Gets purchase failure reason.
		/// </summary>
		public StorePurchaseError ErrorReason => _reason;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(IPurchaseResult result)
			: base(null, false, result.UserState)
		{
			_result = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(IPurchaseResult result, StorePurchaseError failReason, Exception e)
			: base(e, failReason == StorePurchaseError.UserCanceled, result.UserState)
		{
			_result = result;
			_reason = failReason;
		}

		#endregion

		#region IPurchaseResult

		/// <inheritdoc/>
		public PurchaseValidationResult ValidationResult => _result.ValidationResult;

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

		/// <inheritdoc/>
		public bool Restored => _result.Restored;

		#endregion

		#region IStoreOperationInfo

		/// <inheritdoc/>
		public int OperationId => _result.OperationId;

		#endregion
	}
}
