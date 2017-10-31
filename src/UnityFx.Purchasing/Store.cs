// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Store factory and utility methods.
	/// </summary>
	public static class Store
	{
		/// <summary>
		/// Creates a new <see cref="IStoreService"/> instance.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="purchasingModule"/> or <paramref name="storeDelegate"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if an instance of <see cref="IStoreService"/> already exists.</exception>
		public static IStoreService CreateStore(IPurchasingModule purchasingModule, IStoreDelegate storeDelegate)
		{
			if (purchasingModule == null)
			{
				throw new ArgumentNullException(nameof(purchasingModule));
			}

			if (storeDelegate == null)
			{
				throw new ArgumentNullException(nameof(storeDelegate));
			}

			return new StoreService(string.Empty, purchasingModule, storeDelegate);
		}

		/// <summary>
		/// Validates purchase receipt with iOS store and returns the store response string.
		/// </summary>
		/// <param name="receipt">Native iOS store receipt returned by the purchase operation.</param>
		/// <param name="sandboxStore">If <c>true</c> sandbox store should be used.</param>
		public static async Task<string> ValidatePurchaseReceiptIos(string receipt, bool sandboxStore = false)
		{
			var storeUrl = sandboxStore ? "https://sandbox.itunes.apple.com/verifyReceipt" : "https://buy.itunes.apple.com/verifyReceipt";
			var json = string.Format("{{\"receipt-data\":\"{0}\"}}", receipt);
			var ascii = new ASCIIEncoding();
			var postBytes = Encoding.UTF8.GetBytes(json);

			var request = WebRequest.Create(storeUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = postBytes.Length;

			using (var stream = await request.GetRequestStreamAsync())
			{
				await stream.WriteAsync(postBytes, 0, postBytes.Length);
				await stream.FlushAsync();
			}

			var sendResponse = await request.GetResponseAsync();

			using (var streamReader = new StreamReader(sendResponse.GetResponseStream()))
			{
				return await streamReader.ReadToEndAsync();
			}
		}

		/// <summary>
		/// Validates purchase receipt with Google Play store and returns the store response string.
		/// </summary>
		/// <param name="receipt">Native Google Play store receipt returned by the purchase operation.</param>
		public static Task<string> ValidatePurchaseReceiptGooglePlay(string receipt)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates purchase receipt with Amazon store and returns the store response string.
		/// </summary>
		/// <param name="receipt">Native Amazon store receipt returned by the purchase operation.</param>
		public static Task<string> ValidatePurchaseReceiptAmazon(string receipt)
		{
			throw new NotImplementedException();
		}
	}
}
