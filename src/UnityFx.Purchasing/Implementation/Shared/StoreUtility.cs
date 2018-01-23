// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing
{
	[Flags]
	internal enum StoreOperationType
	{
		Initialize = 1,
		InitializeEap = Initialize | 4,
		InitializeApm = Initialize | 8,
		InitializeTap = Initialize | 6,

		Fetch = 2,
		FetchEap = Fetch | 4,
		FetchApm = Fetch | 8,
		FetchTap = Fetch | 6,

		Purchase = 3,
		PurchaseEap = Purchase | 4,
		PurchaseApm = Purchase | 8,
		PurchaseTap = Purchase | 6,
	}

	internal static class StoreUtility
	{
	}
}
