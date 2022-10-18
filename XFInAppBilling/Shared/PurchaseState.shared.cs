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
        /// successful purchase or consume
        /// </summary>
        Purchased,
        /// <summary>
        /// not successfull
        /// </summary>
        Failed,
        /// <summary>
        /// cancelled
        /// </summary>
        Cancelled,
        /// <summary>
        /// restored,  purchase already owned by user
        /// </summary>
        Restored,    
        /// <summary>
        /// in purchase progress
        /// </summary>
        Purchasing,
        /// <summary>
        /// pament not confirmed
        /// </summary>
        PaymentPending,
        /// <summary>
        /// used for consume only
        /// </summary>
        InsufficentQuantity,
        /// <summary>
        ///  In queue, pending external action, IOS
        /// </summary>
        Deferred,
        /// <summary>
        /// unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Problem on the server of the Store
        /// </summary>
        ServerError
    }
}
