// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	public class AppStoreReceipt
	{
		/// <summary>
		/// The app’s bundle identifier.
		/// </summary>
		public string BundleId { get; internal set; }

		/// <summary>
		/// The app’s version number.
		/// </summary>
		public string ApplicationVersion { get; internal set; }

		/// <summary>
		/// The version of the app that was originally purchased.
		/// </summary>
		public string OriginalApplicationVersion { get; internal set; }

		/// <summary>
		/// The receipt for an in-app purchase.
		/// </summary>
		public AppStoreInAppReceipt[] InApp { get; internal set; }
	}
}
