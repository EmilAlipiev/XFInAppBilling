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
}
