using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.XFInAppBilling
{ 
    /// <summary>
    /// Product info specific to Apple Platforms
    /// </summary>
    [Preserve(AllMembers = true)]
    public class ProductAppleExt
    {
        /// <summary>
        /// The identifier of the subscription group to which the subscription belongs.
        /// </summary>
        public string SubscriptionGroupId { get; set; }

        /// <summary>
        /// The period details for products that are subscriptions.
        /// </summary>
        public SubscriptionPeriod SubscriptionPeriod { get; set; }

        /// <summary>
        /// A Boolean value that indicates whether the product is available for family sharing in App Store Connect.
        /// </summary>
        public bool IsFamilyShareable { get; set; }

        /// <summary>
        /// iOS 11.2: gets information about product discunt
        /// </summary>
        public InAppBillingProductDiscount IntroductoryOffer { get; set; } = null;


        /// <summary>
        /// iOS 12.2: gets information about product discunt
        /// </summary>
        public List<InAppBillingProductDiscount> Discounts { get; set; } = null;
    }

    /// <summary>
    /// The details of an introductory offer or a promotional offer for an auto-renewable subscription.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppBillingProductDiscount
    {
        /// <summary>
        /// A string used to uniquely identify a discount offer for a product.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// The type of discount offer.
        /// </summary>
        public ProductDiscountType Type { get; set; } = ProductDiscountType.Unknown;
        /// <summary>
        /// The discount price of the product in the local currency.
        /// </summary>
        public double Price { get; set; } = 0;
        /// <summary>
        /// The locale used to format the discount price of the product.
        /// </summary>
        public string LocalizedPrice { get; set; } = string.Empty;

        /// <summary>
        /// The local currency code
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// The payment mode for this product discount.
        /// </summary>
        public PaymentMode PaymentMode { get; set; } = PaymentMode.Unknown;

        /// <summary>
        /// An integer that indicates the number of periods the product discount is available.
        /// </summary>
        public int NumberOfPeriods { get; set; } = 0;
        /// <summary>
        /// An integer that indicates the number of periods the product discount is available.
        /// </summary>
        public SubscriptionPeriod SubscriptionPeriod { get; set; } = SubscriptionPeriod.Unknown;
    }
    [Preserve(AllMembers = true)]
    public enum ProductDiscountType
    {
        /// <summary>
        /// 
        /// </summary>
        Introductory = 0,
        /// <summary>
        /// 
        /// </summary>
        Subscription = 1,
        /// <summary>
        /// Purchase state unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Gets the current status of the purchase
    /// </summary>
    public enum PaymentMode
    {
        /// <summary>
        /// A constant that indicates that the payment mode is a free trial.
        /// </summary>
        FreeTrial = 0,
        /// <summary>
        /// A constant indicating that the payment mode of a product discount is paid up front.
        /// </summary>
        PayUpFront = 1,
        /// <summary>
        /// Values representing the payment modes for a product discount.
        /// </summary>
        PayAsYouGo = 2,
        /// <summary>
        /// Purchase state unknown
        /// </summary>
        Unknown
    }
}
