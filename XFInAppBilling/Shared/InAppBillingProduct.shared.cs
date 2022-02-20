namespace Plugin.XFInAppBilling
{
    //
    // Summary:
    //     Product being offered
    [Preserve(AllMembers = true)]

    public class InAppBillingProduct
    {
        /// <summary>
        ///   Name of the product
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        ///  Description of the product
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        ///    Product ID or sku
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        ///    Localized Price (not including tax)
        /// </summary>
        public string LocalizedPrice { get; set; }

        /// <summary>
        //     ISO 4217 currency code for price. For example, if price is specified in British
        //     pounds sterling is "GBP".
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        //     Price in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        //     For example, if price is "€7.99", price_amount_micros is "7990000". This value
        //     represents the localized, rounded price for a particular currency.
        /// </summary>
        public long MicrosPrice { get; set; }
        /// <summary>
        /// Gets or sets the localized introductory price.
        /// </summary>
        public string LocalizedIntroductoryPrice { get; set; }
        /// <summary>
        ///  Introductory price of the product in micor-units
        /// </summary>
        public long MicrosIntroductoryPrice { get; set; }
        /// <summary>
        /// Gets a value indicating whether this Plugin.InAppBilling.Abstractions.InAppBillingProduct has introductory price. This is an optional value in the answer from the server, requires a boolean to check if this exists
        /// </summary>
        public bool HasIntroductoryPrice { get; }
        /// <summary>
        /// Returns SKU type.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Returns the icon of the product if present.
        /// </summary>
        public string IconUrl { get; set; }
        /// <summary>
        /// Returns true if sku is rewarded instead of paid.
        /// </summary>
        public bool IsRewarded { get; set; }
        /// <summary>
        /// Formatted introductory price of a subscription, including its currency sign, such as €3.99.
        /// </summary>
        public string IntroductoryPrice { get; set; }
        /// <summary>
        /// The number of subscription billing periods for which the user will be given the introductory price, such as 3.
        /// </summary>
        public int IntroductoryPriceCycles { get; set; }
        /// <summary>
        /// The billing period of the introductory price, specified in ISO 8601 format.
        /// </summary>
        public string IntroductoryPricePeriod { get; set; }
        /// <summary>
        /// Subscription period, specified in ISO 8601 format.
        /// </summary>
        public string SubscriptionPeriod { get; set; }
        /// <summary>
        /// Trial period configured in Google Play Console, specified in ISO 8601 format.
        /// </summary>
        public string FreeTrialPeriod { get; set; }
        /// <summary>
        /// Returns formatted original price of the item, including its currency sign.
        /// if there is a sale this is non-sale price
        /// </summary>
        public string OriginalPrice { get; set; }
        /// <summary>
        /// Returns the original price in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        /// </summary>
        public long OriginalPriceAmountMicros { get; set; }
        /// <summary>
        /// If there is an active sales
        /// </summary>
        public bool IsOnSale { get; set; }
        /// <summary>
        /// Sale end time
        /// </summary>
        public System.DateTimeOffset SaleEndDate { get; set; }
        /// <summary>
        /// Extra information for apple platforms
        /// </summary>
        public ProductAppleExt AppleExtras { get; set; } = null;
    }
}
