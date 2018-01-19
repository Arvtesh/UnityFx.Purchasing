// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event arguments for <see cref="IStoreEvents.PurchaseCompleted"/>.
	/// </summary>
	public class PurchaseCompletedEventArgs : PurchaseEventArgs
	{
		/// <summary>
		/// Returns the purchase result. Read only.
		/// </summary>
		public PurchaseResult Result { get; }

		/// <summary>
		/// Returns purchase failure reason. Read only.
		/// </summary>
		public StorePurchaseError Reason { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the purchase has completed successfully; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsSucceeded => !IsFaulted;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsFaulted => Result is FailedPurchaseResult;

		/// <summary>
		/// Returns <see langword="true"/> if the purchase has failed; <see langword="false"/> otherwise. Read only.
		/// </summary>
		public bool IsCanceled => Reason == StorePurchaseError.UserCanceled;

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(IStoreOperation op, PurchaseResult result)
			: base(op, result.ProductId, result.IsRestored)
		{
			Result = result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseCompletedEventArgs"/> class.
		/// </summary>
		public PurchaseCompletedEventArgs(IStoreOperation op, FailedPurchaseResult result)
			: base(op, result.ProductId, result.IsRestored)
		{
			Result = result;
			Reason = result.Reason;
			Exception = result.Exception;
		}
	}
}
