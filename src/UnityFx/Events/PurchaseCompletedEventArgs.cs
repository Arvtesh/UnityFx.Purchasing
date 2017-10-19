// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event argument for <see cref="IPlatformStore.PurchaseCompleted"/>.
	/// </summary>
	public class PurchaseCompletedEventArgs : EventArgs
	{
		/// <summary>
		/// Returns identifier of the target store. Read only.
		/// </summary>
		public string StoreId { get; }

		/// <summary>
		/// Returns the product selected for purchase. Read only.
		/// </summary>
		public Product Product { get; }

		/// <summary>
		/// Returns <c>true</c> if the purchase was auto-restored; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsRestored { get; }

		/// <summary>
		/// Returns product validation result (<c>null</c> if not available). Read only.
		/// </summary>
		public PurchaseValidationResult ValidationResult { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(Product product, string storeId, bool restored, PurchaseValidationResult validationResult)
		{
			StoreId = storeId;
			Product = product;
			IsRestored = restored;
			ValidationResult = validationResult;
		}
	}
}
