using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.XFInAppBilling
{
    [Preserve(AllMembers = true)]
    public enum PurchaseState
    {
        Unspecified = 0,
        Purchased = 1,
        Pending = 2,
        Failed = 3,
        Cancelled = 4,
        NotAknowledged = 5,
        Restored = 6,
        Refunded = 7,
        Purchasing = 8,
        FreeTrial = 9,
        PaymentPending = 10,
        AlreadyOwned = 11
    }
}
