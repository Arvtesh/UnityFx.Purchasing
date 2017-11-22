// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Event argument for <see cref="IStoreService.StoreInitializationFailed"/>.
	/// </summary>
	public class PurchaseInitializationFailed : EventArgs
	{
		/// <summary>
		/// Returns initialization failure reason. Read only.
		/// </summary>
		public StoreInitializeError Reason { get; }

		/// <summary>
		/// Returns exception that caused the failure (if any). Read only.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PurchaseInitializationFailed"/> class.
		/// </summary>
		public PurchaseInitializationFailed(StoreInitializeError reason, Exception e)
		{
			Reason = reason;
			Exception = e;
		}
	}
}
