// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Purchasing.Validation
{
	/// <summary>
	/// Enumerates possible AppStore subscription expiration reasons.
	/// </summary>
	/// <seealso href="https://developer.apple.com/library/content/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html"/>
	/// <seealso cref="AppStoreInAppReceipt"/>
	/// <seealso cref="AppStoreReceipt"/>
	public enum AppStoreSubcriptionExpirationReason
	{
		/// <summary>
		/// Unknown error.
		/// </summary>
		Unknown,

		/// <summary>
		/// Customer canceled their subscription.
		/// </summary>
		CustomerCanceled,

		/// <summary>
		/// Billing error; for example customer’s payment information was no longer valid.
		/// </summary>
		BillingError,

		/// <summary>
		/// Customer did not agree to a recent price increase.
		/// </summary>
		CustomerRevoked,

		/// <summary>
		/// Product was not available for purchase at the time of renewal.
		/// </summary>
		ProductNotAvailable
	}

	/// <summary>
	/// Enumerates possible AppStore purchase cancellation reasons.
	/// </summary>
	/// <seealso href="https://developer.apple.com/library/content/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html"/>
	/// <seealso cref="AppStoreInAppReceipt"/>
	/// <seealso cref="AppStoreReceipt"/>
	public enum AppStorePurchaseCancellationReason
	{
		/// <summary>
		/// Transaction was canceled for another reason, for example, if the customer made the purchase accidentally.
		/// </summary>
		Unknown,

		/// <summary>
		/// Customer canceled their transaction due to an actual or perceived issue within your app.
		/// </summary>
		AppIssue
	}

	/// <summary>
	/// AppStore in-app receipt.
	/// </summary>
	/// <seealso href="https://developer.apple.com/library/content/releasenotes/General/ValidateAppStoreReceipt/Chapters/ReceiptFields.html"/>
	/// <seealso cref="AppStoreReceipt"/>
	public class AppStoreInAppReceipt : IPurchaseReceipt
	{
		#region interface

		/// <summary>
		/// The number of items purchased. Json field name is <c>quantity</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the <c>quantity</c> property of the <c>SKPayment</c> object stored in the transaction’s payment property.
		/// </remarks>
		public int Quantity { get; internal set; }

		/// <summary>
		/// The product identifier of the item that was purchased. Json field name is <c>product_id</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the <c>productIdentifier</c> property of the <c>SKPayment</c> object stored in the transaction’s payment property.
		/// </remarks>
		public string ProductId { get; internal set; }

		/// <summary>
		/// The transaction identifier of the item that was purchased. Json field name is <c>transaction_id</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the transaction’s <c>transactionIdentifier</c> property. For a transaction that
		/// restores a previous transaction, this value is different from the transaction identifier of the original purchase
		/// transaction. In an auto-renewable subscription receipt, a new value for the transaction identifier is generated
		/// every time the subscription automatically renews or is restored on a new device.
		/// </remarks>
		/// <seealso cref="OriginalTransactionId"/>
		public string TransactionId { get; internal set; }

		/// <summary>
		/// For a transaction that restores a previous transaction, the transaction identifier of the original transaction.
		/// Otherwise, identical to the transaction identifier. Json field name is <c>original_transaction_id</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the original transaction’s <c>transactionIdentifier</c> property. The value is the same
		/// for all receipts that have been generated for a specific subscription. It is useful for relating together multiple
		/// iOS 6 style transaction receipts for the same individual customer’s subscription.
		/// </remarks>
		/// <seealso cref="TransactionId"/>
		public string OriginalTransactionId { get; internal set; }

		/// <summary>
		/// The date and time that the item was purchased. Json field name is <c>purchase_date</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the transaction’s <c>transactionDate</c> property. For a transaction
		/// that restores a previous transaction, the purchase date is the same as the original purchase date.
		/// Use <see cref="OriginalPurchaseDate"/> to get the date of the original transaction. In an auto-renewable
		/// subscription receipt, the purchase date is the date when the subscription was either purchased or
		/// renewed (with or without a lapse). For an automatic renewal that occurs on the expiration date of
		/// the current period, the purchase date is the start date of the next period, which is identical
		/// to the end date of the current period.
		/// </remarks>
		/// <seealso cref="OriginalPurchaseDate"/>
		/// <seealso cref="SubscriptionExpirationDate"/>
		public DateTime PurchaseDate { get; internal set; }

		/// <summary>
		/// For a transaction that restores a previous transaction, the date of the original transaction.
		/// Json field name is <c>original_purchase_date</c>.
		/// </summary>
		/// <remarks>
		/// This value corresponds to the original transaction’s <c>transactionDate</c> property. In an auto-renewable
		/// subscription receipt, this indicates the beginning of the subscription period, even if the subscription has been renewed.
		/// </remarks>
		/// <seealso cref="PurchaseDate"/>
		/// <seealso cref="SubscriptionExpirationDate"/>
		public DateTime OriginalPurchaseDate { get; internal set; }

		/// <summary>
		/// The expiration date for the subscription. Json field name is <c>expires_date</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts. Use this value to identify the date
		/// when the subscription will renew or expire, to determine if a customer should have access to content or
		/// service. After validating the latest receipt, if the subscription expiration date for the latest renewal
		/// transaction is a past date, it is safe to assume that the subscription has expired.
		/// </remarks>
		/// <seealso cref="PurchaseDate"/>
		/// <seealso cref="OriginalPurchaseDate"/>
		public DateTime? SubscriptionExpirationDate { get; internal set; }

		/// <summary>
		/// For an expired subscription, the reason for the subscription expiration. Json field name is <c>expiration_intent</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for a receipt containing an expired auto-renewable subscription. You can use this value to
		/// decide whether to display appropriate messaging in your app for customers to resubscribe.
		/// </remarks>
		public AppStoreSubcriptionExpirationReason? SubscriptionExpirationIntent { get; internal set; }

		/// <summary>
		/// For an expired subscription, whether or not Apple is still attempting to automatically renew the subscription.
		/// Json field name is <c>is_in_billing_retry_period</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts. If the customer’s subscription failed to renew
		/// because the App Store was unable to complete the transaction, this value will reflect whether or not the App Store is
		/// still trying to renew the subscription.
		/// </remarks>
		public bool? SubscriptionRetryFlag { get; internal set; }

		/// <summary>
		/// For a subscription, whether or not it is in the Free Trial period. Json field name is <c>is_trial_period</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts. The value for this key is <see langword="true"/>
		/// if the customer’s subscription is currently in the free trial period, or <see langword="false"/> if not.
		/// </remarks>
		public bool? SubscriptionTrialPeriod { get; internal set; }

		/// <summary>
		/// For a transaction that was canceled by Apple customer support, the time and date of the cancellation.
		/// For an auto-renewable subscription plan that was upgraded, the time and date of the upgrade transaction.
		/// Json field name is <c>cancellation_date</c>.
		/// </summary>
		/// <remarks>
		/// Treat a canceled receipt the same as if no purchase had ever been made. A canceled in-app purchase remains
		/// in the receipt indefinitely. Only applicable if the refund was for a non-consumable product, an auto-renewable
		/// subscription, a non-renewing subscription, or for a free subscription.
		/// </remarks>
		/// <seealso cref="CancellationReason"/>
		public DateTime? CancellationDate { get; internal set; }

		/// <summary>
		/// For a transaction that was cancelled, the reason for cancellation. Json field name is <c>cancellation_reason</c>.
		/// </summary>
		/// <remarks>
		/// Use this value along with the <see cref="CancellationDate"/> to identify possible issues in your app that may lead customers to contact Apple customer support.
		/// </remarks>
		/// <seealso cref="CancellationDate"/>
		public AppStorePurchaseCancellationReason? CancellationReason { get; internal set; }

		/// <summary>
		/// A string that the App Store uses to uniquely identify the application that created the transaction. Json field name is <c>app_item_id</c>.
		/// </summary>
		/// <remarks>
		/// If your server supports multiple applications, you can use this value to differentiate between them. Apps are assigned an identifier
		/// only in the production environment, so this key is not present for receipts created in the test environment. This field is not present for Mac apps.
		/// </remarks>
		public string AppItemId { get; internal set; }

		/// <summary>
		/// An arbitrary number that uniquely identifies a revision of your application. Json field name is <c>version_external_identifier</c>.
		/// </summary>
		/// <remarks>
		/// This key is not present for receipts created in the test environment. Use this value to identify the version of the app that the customer bought.
		/// </remarks>
		public string ExternalVersionId { get; internal set; }

		/// <summary>
		/// The primary key for identifying subscription purchases. Json field name is <c>web_order_line_item_id</c>.
		/// </summary>
		/// <remarks>
		/// This value is a unique ID that identifies purchase events across devices, including subscription renewal purchase events.
		/// </remarks>
		public string WebOrderLineItemId { get; internal set; }

		/// <summary>
		/// The current renewal status for the auto-renewable subscription. Json field name is <c>auto_renew_status</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts, for active or expired subscriptions. The value for this key should not be
		/// interpreted as the customer’s subscription status. You can use this value to display an alternative subscription product in your app, for example,
		/// a lower level subscription plan that the customer can downgrade to from their current plan.
		/// </remarks>
		/// <seealso cref="SubscriptionAutoRenewPreference"/>
		public bool? SubscriptionAutoRenewStatus { get; internal set; }

		/// <summary>
		/// The current renewal preference for the auto-renewable subscription. Json field name is <c>auto_renew_product_id</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts. The value for this key corresponds to the <c>productIdentifier</c> property
		/// of the product that the customer’s subscription renews. You can use this value to present an alternative service level to the customer before
		/// the current subscription period ends.
		/// </remarks>
		/// <seealso cref="SubscriptionAutoRenewStatus"/>
		public string SubscriptionAutoRenewPreference { get; internal set; }

		/// <summary>
		/// The current price consent status for a subscription price increase. Json field name is <c>price_consent_status</c>.
		/// </summary>
		/// <remarks>
		/// This key is only present for auto-renewable subscription receipts if the subscription price was increased without keeping
		/// the existing price for active subscribers. You can use this value to track customer adoption of the new price and take appropriate action.
		/// </remarks>
		public bool? SubscriptionPriceConsentStatus { get; internal set; }

		#endregion

		#region IPurchaseReceipt

		/// <inheritdoc/>
		public DateTime Timestamp => PurchaseDate;

		/// <inheritdoc/>
		public DateTime Timestamp0 => OriginalPurchaseDate;

		#endregion
	}
}
