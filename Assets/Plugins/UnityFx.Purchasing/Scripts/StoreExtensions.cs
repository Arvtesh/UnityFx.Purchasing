// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Purchasing-related extensions.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class StoreExtensions
	{
		#region Product

		/// <summary>
		/// Extracts native platfrom purchase receipt from Unity product.
		/// </summary>
		public static string GetNativeReceipt(this Product product)
		{
			if (string.IsNullOrEmpty(product.receipt))
			{
				return product.receipt;
			}

			return JsonUtility.FromJson<UnifiedReceipt>(product.receipt).Payload;
		}

		/// <summary>
		/// Extracts native platfrom purchase receipt from Unity receipt.
		/// </summary>
		public static string GetNativeReceipt(this Product product, out string storeId)
		{
			if (product.hasReceipt && !string.IsNullOrEmpty(product.receipt))
			{
				var receiptData = JsonUtility.FromJson<UnifiedReceipt>(product.receipt);
				storeId = receiptData.Store;
				return receiptData.Payload;
			}

			storeId = null;
			return product.receipt;
		}

		#endregion

		#region IStoreExtensions
		#endregion
	}
}
