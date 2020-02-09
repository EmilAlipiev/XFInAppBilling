using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// Status used for purchase and consume
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum PurchaseState
    {
        /// <summary>
        /// unknown
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// successful purchase or consume
        /// </summary>
        Purchased = 1,
        /// <summary>
        /// incomplete
        /// </summary>
        Pending = 2,
        /// <summary>
        /// not successfull
        /// </summary>
        Failed = 3,
        /// <summary>
        /// cancelled
        /// </summary>
        Cancelled = 4,
        /// <summary>
        /// pending
        /// </summary>
        NotAknowledged = 5,
        /// <summary>
        /// restored
        /// </summary>
        Restored = 6,
        /// <summary>
        /// refunded
        /// </summary>
        Refunded = 7,
        /// <summary>
        /// in purchase progress
        /// </summary>
        Purchasing = 8,
        /// <summary>
        /// trial purchase
        /// </summary>
        FreeTrial = 9,
        /// <summary>
        /// pament not confirmed
        /// </summary>
        PaymentPending = 10,
        /// <summary>
        /// purchase already owned by user
        /// </summary>
        AlreadyOwned = 11,
        /// <summary>
        /// used for consume only
        /// </summary>
        InsufficentQuantity = 12
    }
}
