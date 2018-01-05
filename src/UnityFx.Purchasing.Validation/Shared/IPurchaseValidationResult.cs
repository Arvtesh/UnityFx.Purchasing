// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// A generic validation result.
	/// </summary>
	public interface IPurchaseValidationResult : IEnumerable<IPurchaseReceipt>
	{
		/// <summary>
		/// Returns raw store validation response. Read only.
		/// </summary>
		string RawResult { get; }

		/// <summary>
		/// Returns response status description. Read only.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the validation succeeded; <see langword="false"/> otherwise. Read only.
		/// </summary>
		bool IsOK { get; }

		/// <summary>
		/// Returns <see langword="true"/> if the validation failed for any reason; <see langword="false"/> otherwise. Read only.
		/// </summary>
		bool IsFailed { get; }
	}
}
