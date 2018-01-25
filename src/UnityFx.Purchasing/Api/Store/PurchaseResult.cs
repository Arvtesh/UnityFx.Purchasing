// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Result of a store purchase.
	/// </summary>
	public struct PurchaseResult : IPurchaseResult
	{
		#region data

		private readonly IPurchaseResult _result;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseResult"/> struct.
		/// </summary>
		internal PurchaseResult(IPurchaseResult result)
		{
			_result = result;
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
