// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// Amazon Store validation result.
	/// </summary>
	/// <seealso cref="AmazonStoreReceipt"/>
	public class AmazonStoreValidationResult
	{
		/// <summary>
		/// Returns raw Amazon Store response. Read only.
		/// </summary>
		public string RawResult { get; }

		/// <summary>
		/// The Amazon Store receipt (id any). Read only.
		/// </summary>
		public AmazonStoreReceipt Receipt { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AmazonStoreValidationResult"/> class.
		/// </summary>
		internal AmazonStoreValidationResult(string rawResponse, AmazonStoreReceipt receipt)
		{
			RawResult = rawResponse;
			Receipt = receipt;
		}
	}
}
