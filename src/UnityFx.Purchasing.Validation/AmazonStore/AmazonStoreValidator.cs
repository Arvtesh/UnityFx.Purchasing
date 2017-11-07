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

		private const string _betaProductValueName = "betaProduct";
		private const string _cancelDateValueName = "cancelDate";
		private const string _parentProductIdValueName = "parentProductId";
		private const string _productIdValueName = "productId";
		private const string _productTypeValueName = "productType";
		private const string _purchaseDateValueName = "purchaseDate";
		private const string _quantityValueName = "quantity";
		private const string _receiptIdValueName = "receiptID";
		private const string _renewalDateValueName = "renewalDate";
		private const string _termValueName = "term";
		private const string _termSkuValueName = "termSku";
		private const string _testTransactionValueName = "testTransaction";

		#endregion

		#region interface

		internal const string TestReceipt = "{\"receiptId\":\"VQuPUcLP1l3C8sBbw_CfmhCbY_zM_aKJ1lklbzZXdF8=:1:11\",\"userId\":\"lXySmLklo4PMPySvWdJ74YqlePwlLjTGcFCO6g6_Q80=\",\"isSandbox\":false}";

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

		internal async static Task<AmazonStoreValidationResult> ValidatePurchaseAsync(string receiptId, string userId, string sharedSecret, bool sandboxStore)
		{
			var responseString = await ValidatePurchaseRawAsync(receiptId, userId, sharedSecret, sandboxStore);
			var receipt = new AmazonStoreReceipt();
			var json = JsonValue.Parse(responseString);

			// required fields
			receipt.ProductId = json[_productIdValueName];
			receipt.ProductType = GetProductType(json[_productTypeValueName]);
			receipt.PurchaseDate = GetDate(json[_purchaseDateValueName]);
			receipt.Quantity = json[_quantityValueName];
			receipt.ReceiptId = json[_receiptIdValueName];

			// optional fields
			if (json.ContainsKey(_renewalDateValueName))
			{
				receipt.RenewalDate =  GetDate(json[_renewalDateValueName]);
			}

			if (json.ContainsKey(_termValueName))
			{
				// TODO
			}

			if (json.ContainsKey(_termSkuValueName))
			{
				receipt.TermSku = json[_termSkuValueName];
			}

			if (json.ContainsKey(_betaProductValueName))
			{
				receipt.IsBetaProduct = json[_betaProductValueName];
			}

			if (json.ContainsKey(_testTransactionValueName))
			{
				receipt.IsTestTransaction = json[_testTransactionValueName];
			}

			return new AmazonStoreValidationResult(0, responseString, receipt);
		}

		internal async static Task<AmazonStoreValidationResult> ValidatePurchaseAsync(string receipt, string sharedSecret)
		{
			var json = JsonValue.Parse(receipt);
			var receiptId = (string)json["receiptId"];
			var userId = (string)json["userId"];
			var isSandbox = (bool)json["isSandbox"];

			return await ValidatePurchaseAsync(receiptId, userId, sharedSecret, isSandbox);
		}

		#endregion

		#region implementation

		private static DateTime GetDate(long ms)
		{
			return new DateTime(ms * 10000, DateTimeKind.Utc);
		}

		private static AmazonStoreProductType GetProductType(string type)
		{
			if (type == "SUBSCRIPTION")
			{
				return AmazonStoreProductType.Subscription;
			}
			else if (type == "ENTITLED")
			{
				return AmazonStoreProductType.Entitled;
			}

			return AmazonStoreProductType.Consumable;
		}

		#endregion
	}
}
