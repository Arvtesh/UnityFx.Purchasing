// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityFx.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing.Examples
{
	public class AppRootBehavior : MonoBehaviour
	{
		private IStoreService _store;

		private void Awake()
		{
			var products = new ProductDefinition[]
			{
			new ProductDefinition("product1", ProductType.Consumable),
			new ProductDefinition("product2", ProductType.Consumable),
			new ProductDefinition("product3", ProductType.NonConsumable),
			new ProductDefinition("product4", "disabledNonConsumable", ProductType.NonConsumable, false),
			new ProductDefinition("product5", "disabledConsumable", ProductType.NonConsumable, false)
			};

			_store = new StoreService(products);
			_store.TraceListeners.Add(new UnityTraceListener());
			_store.TraceSwitch.Level = System.Diagnostics.SourceLevels.All;
		}

		private void Start()
		{
			InitAsync();
		}

		private async void InitAsync()
		{
			try
			{
				await _store.InitializeAsync();
				await _store.PurchaseAsync("product1");
				await _store.PurchaseAsync("product2");
			}
			catch (Exception e)
			{
				Debug.LogException(e, this);
			}
		}
	}
}
