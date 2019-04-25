// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseCompleted"/>.
	/// </summary>
	public class PurchaseCompletedEventArgs : AsyncCompletedEventArgs
	{
		#region data

		private readonly string _productId;
		private readonly Product _product;
		private readonly bool _restored;
		private readonly PurchaseFailureReason? _reason;
		private readonly PurchaseValidationResult _validationResult;

		#endregion

		#region interface

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
		/// Gets the purchase validation status.
		/// </summary>
		public PurchaseValidationStatus ValidationStatus
		{
			get
			{
				return _validationResult.Status;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(int opId, object userState, Product product, PurchaseValidationResult validationResult, bool restored)
			: base(opId, userState, null, false)
		{
			_productId = product.definition.id;
			_product = product;
			_restored = restored;
			_validationResult = validationResult;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(int opId, object userState, PurchaseValidationException e, bool restored)
			: base(opId, userState, e, false)
		{
			_productId = e.Product.definition.id;
			_product = e.Product;
			_restored = restored;
			_validationResult = new PurchaseValidationResult(PurchaseValidationStatus.Failed);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(int opId, object userState, PurchaseException e, bool restored)
			: base(opId, userState, e, e.ErrorCode == PurchaseFailureReason.UserCancelled)
		{
			_productId = e.Product.definition.id;
			_product = e.Product;
			_restored = restored;
			_reason = e.ErrorCode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(int opId, object userState, string productId, Exception e, bool restored)
			: base(opId, userState, e, false)
		{
			_productId = productId;
			_restored = restored;
			_reason = PurchaseFailureReason.Unknown;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(int opId, object userState, Product product, Exception e, bool restored)
			: base(opId, userState, e, false)
		{
			_productId = product.definition.id;
			_product = product;
			_restored = restored;
			_reason = PurchaseFailureReason.Unknown;
		}

		#endregion
	}
}
