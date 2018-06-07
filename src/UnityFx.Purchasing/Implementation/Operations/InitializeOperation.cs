// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	internal class InitializeOperation : StoreConfigOperation
	{
		#region data

		private readonly IStoreListener _storeListener;

		#endregion

		#region interface

		public InitializeOperation(StoreService store, IStoreListener storeListener, AsyncCallback asyncCallback, object asyncState)
			: base(store, StoreOperationType.Initialize, asyncCallback, asyncState)
		{
			_storeListener = storeListener;
			Store.OnInitializeInitiated(Id, asyncState);
		}

		#endregion

		#region StoreConfigOperation

		protected override void Initiate(StoreConfig storeConfig)
		{
			var config = Store.Configure(storeConfig);
			UnityPurchasing.Initialize(_storeListener, config);
		}

		protected override void InvokeCompleted(StoreFetchError reason, Exception e)
		{
			Store.OnInitializeCompleted(reason, e, Id, AsyncState);
		}

		#endregion
	}
}
