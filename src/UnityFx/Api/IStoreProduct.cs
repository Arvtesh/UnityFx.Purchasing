// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityFx.Purchasing
{
	/// <summary>
	/// A minimum interface of an IAP product. It is supposed to be extended/implemented by the library users.
	/// </summary>
	public interface IStoreProduct
	{
		/// <summary>
		/// Return the product identifier. Read only.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Returns the IAP product definition info. Read only.
		/// </summary>
		ProductDefinition Definition { get; }

		/// <summary>
		/// Gets or sets IAP product metadata (set by the store after it is initialized).
		/// </summary>
		ProductMetadata Metadata { get; set; }
	}
}
