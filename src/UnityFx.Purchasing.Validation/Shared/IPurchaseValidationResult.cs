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
		/// Gets raw store validation response.
		/// </summary>
		string RawResult { get; }

		/// <summary>
		/// Gets response status description.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Gets a value indicating whether the validation succeeded.
		/// </summary>
		bool IsOK { get; }

		/// <summary>
		/// Gets a value indicating whether the validation failed for any reason.
		/// </summary>
		bool IsFailed { get; }
	}
}
