// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	public class AppStoreValidationResult
	{
		/// <summary>
		/// Returns raw App Store response. Read only.
		/// </summary>
		public string RawResult { get; }

		/// <summary>
		/// Returns App Store response status code. <c>0</c> means OK. Read only.
		/// </summary>
		public int Status { get; internal set; }

		/// <summary>
		/// Returns App Store response status description. Read only.
		/// </summary>
		public string StatusText { get; internal set; }

		/// <summary>
		/// Returns the store environment identifier. Read only.
		/// </summary>
		public string Environment { get; internal set; }

		/// <summary>
		/// The App Store receipt if any. Read only.
		/// </summary>
		public AppStoreReceipt Receipt { get; internal set; }

		/// <summary>
		/// Returns <c>true</c> if the validation succeeded; <c>false</c> otherwise. Read only.
		/// </summary>
		public bool IsOK => Status == 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="AppStoreValidationResult"/> class.
		/// </summary>
		internal AppStoreValidationResult(string rawResponse)
		{
			RawResult = rawResponse;
		}
	}
}
