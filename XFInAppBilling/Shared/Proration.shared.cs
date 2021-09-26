namespace Plugin.XFInAppBilling
{
    /// <summary>
    ///  When upgrading or downgrading a subscription, you can set the proration mode, or how the change affects your subscribers. 
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum Proration
    {
        /// <summary>
        /// The subscription is upgraded or downgraded immediately. Any time remaining is adjusted based on the price difference, 
        /// and credited toward the new subscription by pushing forward the next billing date. This is the default behavior.
        /// </summary>
        ImmediateWithTimeProration,
        /// <summary>
        /// The subscription is upgraded immediately, and the billing cycle remains the same. 
        /// The price difference for the remaining period is then charged to the user.
        /// </summary>
        ImmediateAndChargeProratedPrice,
        /// <summary>
        /// The subscription is upgraded or downgraded immediately, and the new price is charged when the subscription renews.
        /// The billing cycle remains the same.
        /// </summary>
        ImmediateWithoutProration,
        /// <summary>
        /// The subscription is upgraded or downgraded only when the subscription renews.
        /// </summary>
        Deferred,
        /// <summary>
        /// The subscription is upgraded or downgraded and the user is charged full price for the new entitlement immediately. The remaining value
        /// from the previous subscription is either carried over for the same entitlement, or prorated for time when switching to a different entitlement.
        /// </summary>
        ImmediateAndChargeFullPrice,
    }
}