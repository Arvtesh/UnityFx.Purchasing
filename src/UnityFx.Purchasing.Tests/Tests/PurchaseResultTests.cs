// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace UnityFx.Purchasing.Tests
{
	/// <summary>
	/// Tests for <see cref="PurchaseResult"/>.
	/// </summary>
	public class PurchaseResultTests
	{
		[Fact]
		public void SerializationTest()
		{
			var transaction = new StoreTransaction(null, "a", "b", "c");
			var validationResult = new PurchaseValidationResult(PurchaseValidationStatus.NotAvailable);
			var purchaseResult = new PurchaseResult(transaction, validationResult, true);

			using (var s = new MemoryStream())
			{
				var f = new BinaryFormatter();
				f.Serialize(s, purchaseResult);
				s.Seek(0, SeekOrigin.Begin);
				purchaseResult = f.Deserialize(s) as PurchaseResult;
			}

			Assert.NotNull(purchaseResult.TransactionInfo);
			Assert.NotNull(purchaseResult.ValidationResult);

			Assert.Equal(transaction.TransactionId, purchaseResult.TransactionInfo.TransactionId);
			Assert.Equal(transaction.StoreId, purchaseResult.TransactionInfo.StoreId);
			Assert.Equal(transaction.Receipt, purchaseResult.TransactionInfo.Receipt);
			Assert.Equal(validationResult.Status, purchaseResult.ValidationResult.Status);
		}
	}
}
