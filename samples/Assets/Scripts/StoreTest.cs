using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing.Tests
{
	public class StoreTest : MonoBehaviour, IStoreDelegate
	{
		#region MonoBehaviour

		private void Awake()
		{
			var purchasingModule = StandardPurchasingModule.Instance();
			purchasingModule.useFakeStoreAlways = true;
			purchasingModule.useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;

			var store = Store.CreateStore(purchasingModule, this);
			store.TraceListeners.Add(new UnityTraceListener());
			store.PurchaseAsync("r1");
		}

		#endregion

		#region IStoreDelegate

		public IDisposable BeginWait()
		{
			return null;
		}

		public Task<StoreConfig> GetStoreConfigAsync()
		{
			var products = new ProductDefinition[]
			{
				new ProductDefinition("1", ProductType.Consumable),
				new ProductDefinition("2", ProductType.NonConsumable),
				new ProductDefinition("3", ProductType.Subscription)
			};

			return Task.FromResult(new StoreConfig(products));
		}

		public Task<PurchaseValidationResult> ValidatePurchaseAsync(IStoreProduct product, StoreTransaction transactionInfo)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
