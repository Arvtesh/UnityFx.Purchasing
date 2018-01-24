// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing.Tests
{
	internal class TestStoreService : StoreService
	{
		public TestStoreService()
			: base(null, new TestPurchasingModule())
		{
		}

		protected override Task<StoreConfig> GetStoreConfigAsync()
		{
			var products = new ProductDefinition[]
			{
				new ProductDefinition("test_product_1", ProductType.Consumable),
				new ProductDefinition("test_product_2", ProductType.NonConsumable),
				new ProductDefinition("test_product_3", ProductType.Subscription)
			};

			var config = new StoreConfig(products);
			return Task.FromResult(config);
		}

		protected override bool IsPlatformSupported()
		{
			return true;
		}
	}
}
