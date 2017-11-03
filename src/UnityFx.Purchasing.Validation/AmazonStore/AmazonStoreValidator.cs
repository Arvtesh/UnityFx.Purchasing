// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Json;
using System.IO;
using System.Net;
#if NETSTANDARD1_3
using System.Net.Http;
#endif
using System.Text;
using System.Threading.Tasks;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// Amazon Store validation helpers. 
	/// </summary>
	internal static class AmazonStoreValidator
	{
		#region data
		#endregion

		#region interface

		internal static async Task<string> ValidatePurchaseRawAsync(string receiptId, string userId, string sharedSecret, bool sandboxStore)
		{
			var url = sandboxStore ?
				"https://appstore-sdk.amazon.com/RVSSandbox/version/1.0/verifyReceiptId/developer/{0}/user/{1}/receiptId/{2}" :
				"https://appstore-sdk.amazon.com/version/1.0/verifyReceiptId/developer/{0}/user/{1}/receiptId/{2}";

#if NET46
			var request = WebRequest.Create(url);
			request.Method = "GET";

			var sendResponse = await request.GetResponseAsync();

			using (var streamReader = new StreamReader(sendResponse.GetResponseStream()))
			{
				return await streamReader.ReadToEndAsync();
			}
#else
			using (var httpClient = new HttpClient())
			{
				return await httpClient.GetStringAsync(url);
			}
#endif
		}

		internal static Task<AmazonStoreValidationResult> ValidatePurchaseAsync(string receiptId, string userId, string sharedSecret, bool sandboxStore)
		{
			throw new NotImplementedException();
		}

		internal static Task<AmazonStoreValidationResult> ValidatePurchaseAsync(string receipt, string sharedSecret)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region implementation
		#endregion
	}
}
