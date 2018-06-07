// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreService.PurchaseInitiated"/>.
	/// </summary>
	public class PurchaseInitiatedEventArgs : EventArgs
	{
		#region data

		private readonly int _id;
		private readonly object _userState;
		private readonly string _productId;
		private readonly bool _restored;

		#endregion

		#region interface

		/// <summary>
		/// Gets identifier of the dismiss operation.
		/// </summary>
		public int OperationId => _id;

		/// <summary>
		/// Gets user-defined data assosisated with the operation.
		/// </summary>
		public object UserState => _userState;

		/// <summary>
		/// Gets the product identifier.
		/// </summary>
		public string ProductId => _productId;

		/// <summary>
		/// Gets a value indicating whether the purchase was auto-restored.
		/// </summary>
		public bool Restored => _restored;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInitiatedEventArgs"/> class.
		/// </summary>
		public PurchaseInitiatedEventArgs(string productId, bool restored, int opId, object userState)
		{
			_id = opId;
			_userState = userState;
			_productId = productId;
			_restored = restored;
		}

		#endregion
	}
}
