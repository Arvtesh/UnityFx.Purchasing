// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityFx.Async;
using Xunit;

namespace UnityFx.Purchasing
{
	public class StoreServiceTests : IDisposable
	{
		private readonly StoreService _store = new TestStoreService();
		public void Dispose() => _store.Dispose();

		[Fact]
		public void Initialize_Completes()
		{
			// Arrange

			// Act

			// Assert

		}

	}
}
