// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// Implements a <see cref="IStoreService"/> that operates on a predefined set of products and implements local purchase validation.
	/// </summary>
	internal class StaticStore : StoreService
	{
		#region data

		private readonly IEnumerable<ProductDefinition> _products;
		private readonly byte[] _appleTangle;
		private readonly byte[] _googleTangle;

		#endregion

		#region interface

		internal StaticStore(string name, IPurchasingModule purchasingModule, IEnumerable<ProductDefinition> products, byte[] appleTangle, byte[] googleTangle)
			: base(name, purchasingModule)
		{
			_products = products;
			_appleTangle = appleTangle;
			_googleTangle = googleTangle;
		}

		#endregion

		#region StoreService

		protected internal sealed override void GetStoreConfig(Action<StoreConfig> onSuccess, Action<Exception> onFailure)
		{
			onSuccess(new StoreConfig(_products));
		}

		protected internal sealed override bool ValidatePurchase(StoreTransaction transaction, Action<PurchaseValidationResult> resultDelegate)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				if (_appleTangle != null || _googleTangle != null)
				{
					// NOTE: the Unity built-in validator will throw on invalid receipt and the base class implementation would treat that as failed validation
					////var validator = new CrossPlatformValidator(_googleTangle, _appleTangle, Application.identifier);
					////validator.Validate(transaction.Product.receipt);
					resultDelegate(new PurchaseValidationResult(PurchaseValidationStatus.Ok));
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
