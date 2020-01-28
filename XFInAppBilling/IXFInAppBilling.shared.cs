using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.XFInAppBilling
{   
    [Preserve(AllMembers = true)]
    public interface IXFInAppBilling
    {
        /// <summary>
        /// Gets all the available/purchasable IAPs and Subscriptions
        /// </summary>
        /// <param name="ProductIds">IAP and/or Subscription Ids</param>
        /// <param name="itemType">only used for Google, not used for UWP, Amazon</param>
        /// <returns></returns>
        Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase);
        /// <summary>
        /// Initiates a purchase flow and completes it
        /// </summary>
        /// <param name="productId">in app purchase or subscription Id to be purchased</param>
        /// <param name="itemType">only used for Google, not used for UWP, Amazon</param>
        /// <param name="payload">only used for Google, not used for UWP, Amazon</param>
        /// <param name="verifyPurchase">only used for Google, not used for UWP, Amazon</param>
        /// <returns></returns>
        Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, bool verifyPurchase = false);
        /// <summary>
        /// Gets all current purchases with status information
        /// </summary>
        /// <param name="itemType">only used for Google, not used for UWP, Amazon</param>
        /// <returns></returns>
        Task<List<PurchaseResult>> GetPurchases(ItemType itemType = ItemType.InAppPurchase);
        /// <summary>
        /// Checks if user has any active subscription. It mostly calls GetPurchases and filters by given subscriptionId
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="itemType">only used for Google, not used for UWP, Amazon</param>
        /// <returns></returns>
        Task<bool> CheckIfUserHasActiveSubscriptionAsync(string subscriptionId, ItemType itemType = ItemType.InAppPurchase);
        /// <summary>
        /// Gets all the purchased IAPs for the current user and app. no purchase status is returned
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        Task<List<PurchaseResult>> GetPurchaseHistory(ItemType itemType = ItemType.InAppPurchase);
        /// <summary>
        /// Initiate Store or Native Api. Pre-checks IAP availability as well.
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectAsync();
        /// <summary>
        /// Disconnects from the native api
        /// </summary>
        /// <returns></returns>
        bool Disconnect();

        // Task<bool> GetUserData(string subscriptionStoreId);
    }
}
