// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	public class TestStoreController : IStoreController
	{
		public ProductCollection products => throw new NotImplementedException();

		public void ConfirmPendingPurchase(Product product)
		{
		}

		public void FetchAdditionalProducts(HashSet<ProductDefinition> products, Action successCallback, Action<InitializationFailureReason> failCallback)
		{
			throw new NotImplementedException();
		}

		public void InitiatePurchase(Product product, string payload)
		{
			InitiatePurchase(product.definition.id);
		}

		public void InitiatePurchase(string productId, string payload)
		{
			InitiatePurchase(productId);
		}

		public void InitiatePurchase(Product product)
		{
			InitiatePurchase(product.definition.id);
		}

		public void InitiatePurchase(string productId)
		{
			throw new NotImplementedException();
		}
	}
}
