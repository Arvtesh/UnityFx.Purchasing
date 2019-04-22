// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a store purchase.
	/// </summary>
	public class PurchaseResult
	{
		#region data

		private readonly Product _product;
		private readonly bool _restored;
		private readonly PurchaseValidationResult _validationResult;

		#endregion

		#region interface

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
		/// Initializes a new instance of the <see cref="PurchaseResult"/> class.
		/// </summary>
		public PurchaseResult(Product product, PurchaseValidationResult validationResult, bool restored)
		{
			_product = product;
			_restored = restored;
			_validationResult = validationResult;
		}

		#endregion
	}
}
