// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// Amazon Store validation result.
	/// </summary>
	/// <seealso cref="AmazonStoreReceipt"/>
	public class AmazonStoreValidationResult : IPurchaseValidationResult
	{
		#region interface

		/// <summary>
		/// Gets the status code.
		/// </summary>
		public int StatusCode { get; }

		/// <summary>
		/// Gets Amazon Store receipt (id any).
		/// </summary>
		public AmazonStoreReceipt Receipt { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AmazonStoreValidationResult"/> class.
		/// </summary>
		internal AmazonStoreValidationResult(int status, string rawResponse, AmazonStoreReceipt receipt)
		{
			StatusCode = status;
			RawResult = rawResponse;
			Receipt = receipt;
		}

		#endregion

		#region IPurchaseValidationResult

		/// <inheritdoc/>
		public string RawResult { get; }

		/// <inheritdoc/>
		public string Status { get; internal set; }

		/// <inheritdoc/>
		public bool IsOK => StatusCode == 0;

		/// <inheritdoc/>
		public bool IsFailed => StatusCode != 0;

		#endregion

		#region IEnumerable

		/// <inheritdoc/>
		public IEnumerator<IPurchaseReceipt> GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		#endregion

		#region implementation

		private IEnumerator<IPurchaseReceipt> GetEnumeratorInternal()
		{
			if (Receipt != null)
			{
				yield return Receipt;
			}
		}

		#endregion
	}
}
