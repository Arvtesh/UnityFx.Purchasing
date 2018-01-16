﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// An initialize operation.
	/// </summary>
	internal class InitializeOperation : StoreConfigOperation
	{
		#region data

		private readonly IPurchasingModule _purchasingModule;
		private readonly IStoreListener _storeListener;

		#endregion

		#region interface

		public InitializeOperation(StoreOperationContainer parent, IPurchasingModule purchasingModule, IStoreListener storeListener)
			: base(parent, TraceEventId.Initialize)
		{
			_purchasingModule = purchasingModule;
			_storeListener = storeListener;

			Store.InvokeInitializeInitiated();
		}

		#endregion

		#region StoreConfigOperation

		protected override void Initiate(StoreConfig storeConfig)
		{
			var configurationBuilder = ConfigurationBuilder.Instance(_purchasingModule);
			configurationBuilder.AddProducts(storeConfig.Products);

			UnityPurchasing.Initialize(_storeListener, configurationBuilder);
		}

		protected override void InvokeCompleted()
		{
			Store.InvokeInitializeCompleted();
		}

		protected override void InvokeFailed(StoreFetchError reason, Exception e)
		{
			Store.InvokeInitializeFailed(reason, e);
		}

		#endregion
	}
}