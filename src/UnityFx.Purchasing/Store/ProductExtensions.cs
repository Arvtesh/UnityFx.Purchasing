// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Extensions of <see cref="Product"/>.
	/// </summary>
	public static class ProductExtensions
	{
		[Serializable]
		private struct UnityReceiptData
		{
			public string Store;
			public string Payload;

			public UnityReceiptData(string store, string payload)
			{
				Store = store;
				Payload = payload;
			}
		}

		/// <summary>
		/// Extracts native platfrom purchase receipt from Unity product.
		/// </summary>
		public static string GetNativeReceipt(this Product product)
		{
			if (string.IsNullOrEmpty(product.receipt))
			{
				return product.receipt;
			}

			return JsonUtility.FromJson<UnityReceiptData>(product.receipt).Payload;
		}

		/// <summary>
		/// Extracts native platfrom purchase receipt from Unity receipt.
		/// </summary>
		public static string GetNativeReceipt(this Product product, out string storeId)
		{
			if (product.hasReceipt && !string.IsNullOrEmpty(product.receipt))
			{
				var receiptData = JsonUtility.FromJson<UnityReceiptData>(product.receipt);
				storeId = receiptData.Store;
				return receiptData.Payload;
			}

			storeId = null;
			return product.receipt;
		}
	}
}
