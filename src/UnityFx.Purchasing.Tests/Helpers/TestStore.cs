// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing.Tests
{
	internal class TestStore : IStore
	{
		private IStoreCallback _storeCallback;

		public void Initialize(IStoreCallback callback)
		{
			_storeCallback = callback;
		}

		public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
		{
			var products = new List<ProductDescription>();

			foreach (var product in productDefinitions)
			{
				var metadata = new ProductMetadata("$123.45", "Fake title for " + product.id, "Fake description", "USD", 123.45m);
				products.Add(new ProductDescription(product.storeSpecificId, metadata));
			}

			_storeCallback.OnProductsRetrieved(products);
		}

		public void Purchase(ProductDefinition product, string developerPayload)
		{
			_storeCallback.OnPurchaseSucceeded(product.storeSpecificId, "{ \"this\" : \"is a fake receipt\" }", Guid.NewGuid().ToString());
		}

		public void FinishTransaction(ProductDefinition product, string transactionId)
		{
		}
	}
}
