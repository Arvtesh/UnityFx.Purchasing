// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using NSubstitute;

namespace UnityFx.Purchasing.Tests
{
	/// <summary>
	/// Tests for <see cref="StoreTransaction"/>.
	/// </summary>
	public class StoreTransactionTests
	{
		[Fact]
		public void SerializationTest()
		{
			var transactionId = "123456";
			var storeId = "test_store";
			var receipt = "test receipt";
			var transaction = new StoreTransaction(null, transactionId, storeId, receipt, true);

			using (var s = new MemoryStream())
			{
				var f = new BinaryFormatter();
				f.Serialize(s, transaction);
				s.Seek(0, SeekOrigin.Begin);
				transaction = f.Deserialize(s) as StoreTransaction;
			}

			Assert.Equal(transactionId, transaction.TransactionId);
			Assert.Equal(storeId, transaction.StoreId);
			Assert.Equal(receipt, transaction.Receipt);
			Assert.True(transaction.IsRestored);
		}
	}
}
