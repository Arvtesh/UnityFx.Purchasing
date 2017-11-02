﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading.Tasks;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	public static class ReceiptValidator
	{
		/// <summary>
		/// Validates purchase receipt with AppStore and returns the store.
		/// </summary>
		/// <param name="receipt">Native iOS store receipt returned by the purchase operation.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="receipt"/> value is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="receipt"/> value is an empty string.</exception>
		public static Task<AppStoreValidationResult> ValidateAppStoreReceiptAsync(string receipt)
		{
			ThrowIfInvalidReceipt(receipt);
			return AppStoreValidator.ValidateReceiptAsync(receipt);
		}

		/// <summary>
		/// Validates purchase receipt with Google Play store and returns the store response string.
		/// </summary>
		/// <param name="receipt">Native Google Play store receipt returned by the purchase operation.</param>
		public static Task<string> ValidateGooglePlayReceiptAsync(string receipt)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates purchase receipt with Amazon store and returns the store response string.
		/// </summary>
		/// <param name="receipt">Native Amazon store receipt returned by the purchase operation.</param>
		public static Task<string> ValidateAmazonReceiptAsync(string receipt)
		{
			throw new NotImplementedException();
		}

		private static void ThrowIfInvalidReceipt(string receipt)
		{
			if (receipt == null)
			{
				throw new ArgumentNullException(nameof(receipt));
			}

			if (string.IsNullOrEmpty(receipt))
			{
				throw new ArgumentException("Invalid receipt value", nameof(receipt));
			}
		}
	}
}