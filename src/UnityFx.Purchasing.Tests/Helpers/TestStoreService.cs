// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityFx.Async;

namespace UnityFx.Purchasing
{
	public class TestStoreService : StoreService, IStoreCallback
	{
		#region data

		private readonly TestStore _store = new TestStore(true);

		#endregion

		#region interface

		public TestStoreService()
			: base(new TestPurchasingModule())
		{
			_store.Initialize(this);
		}

		#endregion

		#region StoreService

		protected override IAsyncOperation<StoreConfig> GetStoreConfig()
		{
			var products = new ProductDefinition[]
			{
				new ProductDefinition("test_product_1", ProductType.Consumable),
				new ProductDefinition("test_product_2", ProductType.Consumable),
				new ProductDefinition("test_product_3", ProductType.Consumable),
				new ProductDefinition("test_product_4", ProductType.NonConsumable),
				new ProductDefinition("test_product_5", ProductType.Subscription)
			};

			return AsyncResult.FromResult(new StoreConfig(products));
		}

		protected override void OnInitialize(ConfigurationBuilder configurationBuilder, StoreConfig storeConfig)
		{
			
		}

		#endregion

		#region IStoreCallback

		public ProductCollection products => throw new NotImplementedException();

		public bool useTransactionLog { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void OnProductsRetrieved(List<ProductDescription> products)
		{
			throw new NotImplementedException();
		}

		public void OnPurchaseFailed(PurchaseFailureDescription desc)
		{
			throw new NotImplementedException();
		}

		public void OnPurchaseSucceeded(string storeSpecificId, string receipt, string transactionIdentifier)
		{
			throw new NotImplementedException();
		}

		public void OnSetupFailed(InitializationFailureReason reason)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
