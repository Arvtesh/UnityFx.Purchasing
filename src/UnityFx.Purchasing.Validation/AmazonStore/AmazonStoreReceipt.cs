// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="AmazonStoreReceipt"/>
	public enum AmazonStoreProductType
	{
		/// <summary>
		/// 
		/// </summary>
		Consumable,

		/// <summary>
		/// 
		/// </summary>
		Subscription,

		/// <summary>
		/// 
		/// </summary>
		Entitled
	}

	/// <summary>
	/// Amazon Store purchase receipt.
	/// </summary>
	/// <seealso href="https://developer.amazon.com/docs/in-app-purchasing/iap-rvs-for-android-apps.html"/>
	/// <seealso cref="AmazonStoreValidationResult"/>
	public class AmazonStoreReceipt : IPurchaseReceipt
	{
		#region interface

		/// <summary>
		/// The SKU that you defined for this item in your app. Json field name is <c>productId</c>.
		/// </summary>
		public string ProductId { get; internal set; }

		/// <summary>
		/// Null. Reserved for future use. Json field name is <c>parentProductId</c>.
		/// </summary>
		public string ParentProductId { get; internal set; }

		/// <summary>
		/// Type of product purchased. Json field name is <c>productType</c>.
		/// </summary>
		public AmazonStoreProductType ProductType { get; internal set; }

		/// <summary>
		/// Quantity of items purchased or <c>0</c>. Json field name is <c>quantity</c>.
		/// </summary>
		public int Quantity { get; internal set; }

		/// <summary>
		/// The date of the purchase. For subscription items, represents the initial purchase date,
		/// not the purchase date of subsequent renewals. Json field name is <c>receipt_creation_date</c>.
		/// </summary>
		public DateTime PurchaseDate { get; internal set; }

		/// <summary>
		/// The date the purchase was cancelled, or the subscription expired. The field is <see langword="null"/>
		/// if the purchase was not cancelled. Json field name is <c>cancelDate</c>.
		/// </summary>
		public DateTime? CancelDate { get; internal set; }

		/// <summary>
		/// The date that a subscription purchase needs to be renewed. Json field name is <c>renewalDate</c>.
		/// </summary>
		public DateTime? RenewalDate { get; internal set; }

		/// <summary>
		/// Unique identifier for the purchase. Json field name is <c>receiptID</c>.
		/// </summary>
		public string ReceiptId { get; internal set; }

		/// <summary>
		/// Duration that a subscription IAP will remain valid (the term starts on the date of purchase).
		/// Json field name is <c>term</c>.
		/// </summary>
		public TimeSpan Term { get; internal set; }

		/// <summary>
		/// Unique SKU that corresponds to the subscription term. Json field name is <c>termSku</c>.
		/// </summary>
		public string TermSku { get; internal set; }

		/// <summary>
		/// Indicates whether this purchase was executed as a part of Amazon’s publishing and testing process.
		/// Json field name is <c>testTransaction</c>.
		/// </summary>
		public bool IsTestTransaction { get; internal set; }

		/// <summary>
		/// Indicates whether the product purchased is a Live App Testing product.
		/// Json field name is <c>betaProduct</c>.
		/// </summary>
		public bool IsBetaProduct { get; internal set; }

		#endregion

		#region IPurchaseReceipt

		/// <inheritdoc/>
		public string TransactionId => ReceiptId;

		/// <inheritdoc/>
		public DateTime Timestamp => RenewalDate ?? PurchaseDate;

		/// <inheritdoc/>
		public DateTime Timestamp0 => PurchaseDate;

		#endregion
	}
}
