// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseInitiated"/>.
	/// </summary>
	public class PurchaseInitiatedEventArgs : AsyncInitiatedEventArgs
	{
		#region data

		private readonly string _productId;
		private readonly bool _restored;

		#endregion

		#region interface

		/// <summary>
		/// Gets the product identifier.
		/// </summary>
		public string ProductId
		{
			get
			{
				return _productId;
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
		/// Initializes a new instance of the <see cref="PurchaseInitiatedEventArgs"/> class.
		/// </summary>
		public PurchaseInitiatedEventArgs(int opId, object userState, string productId, bool restored)
			: base(opId, userState)
		{
			_productId = productId;
			_restored = restored;
		}

		#endregion
	}
}
