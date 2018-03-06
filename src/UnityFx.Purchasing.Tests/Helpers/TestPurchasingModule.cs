// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace UnityFx.Purchasing
{
	public class TestPurchasingModule : IPurchasingModule
	{
		public void Configure(IPurchasingBinder binder)
		{
			binder.RegisterStore("TestStore", new TestStore(false));
		}
	}
}
