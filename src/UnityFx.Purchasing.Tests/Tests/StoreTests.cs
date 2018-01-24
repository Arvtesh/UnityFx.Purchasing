// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using Xunit;
using Xunit.Extensions;

namespace UnityFx.Purchasing.Tests
{
	public class StoreTests
	{
		[Fact]
		public void StoreCreationSucceeds()
		{
			var store = new TestStoreService();
			store.InitializeAsync(null);
		}
	}
}
