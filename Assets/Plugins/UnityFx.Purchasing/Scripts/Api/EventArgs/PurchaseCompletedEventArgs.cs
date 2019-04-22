// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseCompleted"/>.
	/// </summary>
	public class PurchaseCompletedEventArgs : AsyncCompletedEventArgs
	{
		#region data

		private readonly int _id;
		private readonly string _productId;
		private readonly Product _product;
		private readonly bool _restored;
		private readonly PurchaseFailureReason? _reason;
		private readonly PurchaseValidationResult _validationResult;

		#endregion

		#region interface

		/// <summary>
		/// Gets identifier of the purchase operation.
		/// </summary>
		public int OperationId
		{
			get
			{
				return _id;
			}
		}

		/// <summary>
		/// Gets identifier of the product.
		/// </summary>
		public string ProductId
		{
			get
			{
				return _productId;
			}
		}

		/// <summary>
		/// Gets the product.
		/// </summary>
		public Product Product
		{
			get
			{
				return _product;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the purchase was auto-restored.
		/// </summary>
		public bool Restored
		{
			get
			{
				return _restored;
			}
		}

		/// <summary>
		/// Gets purchase failure reason.
		/// </summary>
		public PurchaseFailureReason? ErrorCode
		{
			get
			{
				return _reason;
			}
		}

		/// <summary>
		/// Gets the purchase validation result.
		/// </summary>
		public PurchaseValidationResult ValidationResult
		{
			get
			{
				return _validationResult;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the purchase is successful.
		/// </summary>
		public bool IsCompletedSuccessfully
		{
			get
			{
				return Error == null && !Cancelled && _product != null && _reason == null && _validationResult.Status == PurchaseValidationStatus.Ok;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(string productId, Product product, PurchaseValidationResult validationResult, bool restored, int opId, object userState)
			: base(null, false, userState)
		{
			_id = opId;
			_productId = productId;
			_product = product;
			_restored = restored;
			_validationResult = validationResult;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(string productId, Product product, PurchaseValidationResult validationResult, PurchaseFailureReason? failReason, Exception e, bool restored, int opId, object userState)
			: base(e, failReason == PurchaseFailureReason.UserCancelled, userState)
		{
			_id = opId;
			_productId = productId;
			_product = product;
			_restored = restored;
			_reason = failReason;
			_validationResult = validationResult;
		}

		#endregion
	}
}
