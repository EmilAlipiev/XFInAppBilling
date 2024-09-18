using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.XFInAppBilling;
[Preserve(AllMembers = true)]
public class ProductAndroidEx
{
    /// <summary>
    /// The period details for products that are subscriptions.
    /// </summary>
    public List<SubscriptionOfferDetail> SubscriptionOfferDetails { get; set; }
}
