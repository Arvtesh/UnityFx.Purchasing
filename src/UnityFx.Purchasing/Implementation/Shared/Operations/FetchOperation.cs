﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A fetch operation.
	/// </summary>
	internal class FetchOperation : StoreConfigOperation
	{
		#region data

		private readonly Action _fetchComplete;
		private readonly Action<InitializationFailureReason> _fetchFailed;

		#endregion

		#region interface

		public FetchOperation(StoreOperationContainer parent, Action onComplete, Action<InitializationFailureReason> onFailed)
			: base(parent, TraceEventId.Fetch)
		{
			_fetchComplete = onComplete;
			_fetchFailed = onFailed;

			Store.InvokeFetchInitiated();
		}

		#endregion

		#region StoreConfigOperation

		protected override void Initiate(StoreConfig storeConfig)
		{
			var productSet = new HashSet<ProductDefinition>(storeConfig.Products);
			Store.Controller.FetchAdditionalProducts(productSet, _fetchComplete, _fetchFailed);
		}

		protected override void InvokeCompleted()
		{
			Store.InvokeFetchCompleted();
		}

		protected override void InvokeFailed(StoreFetchError reason, Exception e)
		{
			Store.InvokeFetchFailed(reason, e);
		}

		#endregion
	}
}