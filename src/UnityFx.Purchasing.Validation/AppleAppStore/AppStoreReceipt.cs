// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// App Store receipt.
	/// </summary>
	/// <seealso href="https://developer.apple.com/library/content/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html"/>
	public class AppStoreReceipt
	{
		/// <summary>
		/// The app’s bundle identifier. Json field name is <c>bundle_id</c>.
		/// </summary>
		/// <remarks>
		/// This corresponds to the value of <c>CFBundleIdentifier</c> in the <c>Info.plist</c> file.
		/// Use this value to validate if the receipt was indeed generated for your app.
		/// </remarks>
		public string BundleId { get; internal set; }

		/// <summary>
		/// The app’s version number. Json field name is <c>application_version</c>.
		/// </summary>
		/// <remarks>
		/// This corresponds to the value of <c>CFBundleVersion</c> (in iOS) or <c>CFBundleShortVersionString</c>
		/// in the <c>Info.plist</c>.
		/// </remarks>
		/// <seealso cref="OriginalAppVersion"/>
		public string AppVersion { get; internal set; }

		/// <summary>
		/// The version of the app that was originally purchased. Json field name is <c>original_application_version</c>.
		/// </summary>
		/// <remarks>
		/// This corresponds to the value of <c>CFBundleVersion</c> (in iOS) or <c>CFBundleShortVersionString</c>
		/// (in macOS) in the <c>Info.plist</c> file when the purchase was originally made. In the sandbox environment,
		/// the value of this field is always “1.0”.
		/// </remarks>
		/// <seealso cref="AppVersion"/>
		public string OriginalAppVersion { get; internal set; }

		/// <summary>
		/// The date when the app receipt was created. Json field name is <c>receipt_creation_date</c>.
		/// </summary>
		/// <remarks>
		/// When validating a receipt, use this date to validate the receipt’s signature.
		/// </remarks>
		/// <seealso cref="ExpirationDate"/>
		public DateTime CreationDate { get; internal set; }

		/// <summary>
		/// The date that the app receipt expires. Json field name is <c>expiration_date</c>.
		/// </summary>
		/// <remarks>
		/// This key is present only for apps purchased through the Volume Purchase Program.
		/// If this key is not present, the receipt does not expire. When validating a receipt,
		/// compare this date to the current date to determine whether the receipt is expired.
		/// Do not try to use this date to calculate any other information, such as the time
		/// remaining before expiration.
		/// </remarks>
		/// <seealso cref="CreationDate"/>
		public DateTime? ExpirationDate { get; internal set; }

		/// <summary>
		/// The receipt for an in-app purchase. An empty array is a valid receipt. Json field name is <c>in_app</c>.
		/// </summary>
		/// <remarks>
		/// <para>In the JSON file, the value of this key is an array containing all in-app purchase
		/// receipts based on the in-app purchase transactions present in the input base-64 receipt-data.
		/// For receipts containing auto-renewable subscriptions, check the value of the <c>latest_receipt_info</c>
		/// key to get the status of the most recent renewal.</para>
		/// <para>The in-app purchase receipt for a consumable product is added to the receipt when the purchase is made.
		/// It is kept in the receipt until your app finishes that transaction. After that point, it is removed from
		/// the receipt the next time the receipt is updated - for example, when the user makes another purchase or
		/// if your app explicitly refreshes the receipt.</para>
		/// <para>The in-app purchase receipt for a non-consumable product, auto-renewable subscription, non-renewing subscription,
		/// or free subscription remains in the receipt indefinitely.</para>
		/// </remarks>
		public AppStoreInAppReceipt[] InApp { get; internal set; }
	}
}
