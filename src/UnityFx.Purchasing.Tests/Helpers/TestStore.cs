// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	public class TestStore : IStore
	{
		private bool _asyncOperations;
		private IStoreCallback _storeCallback;

		public TestStore(bool asyncOperations)
		{
			_asyncOperations = asyncOperations;
		}

		public void Initialize(IStoreCallback callback)
		{
			_storeCallback = callback;
		}

		public async void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
		{
			var result = new List<ProductDescription>(products.Count);

			if (_asyncOperations)
			{
				await Task.Delay(1);
			}

			foreach (var product in products)
			{
				var metadata = new ProductMetadata("$0.99", "Fake title for " + product.id, "Fake product description", "USD", 0.99m);
				result.Add(new ProductDescription(product.storeSpecificId, metadata));
			}

			_storeCallback.OnProductsRetrieved(result);
		}

		public async void Purchase(ProductDefinition product, string developerPayload)
		{
			if (_asyncOperations)
			{
				await Task.Delay(1);
			}

			_storeCallback.OnPurchaseSucceeded(product.storeSpecificId, "{ \"Fake\": \"receipt\" }", Guid.NewGuid().ToString());
		}

		public void FinishTransaction(ProductDefinition product, string transactionId)
		{
		}
	}
}
