namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// Product item type
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum ItemType
    {
        /// <summary>
        /// Single purchase (managed)
        /// </summary>
        InAppPurchase,
        /// <summary>
        /// On going subscription
        /// </summary>
        Subscription,
        /// <summary>
        /// Consumable IAp
        /// </summary>
        InAppPurchaseConsumable
    }
}
