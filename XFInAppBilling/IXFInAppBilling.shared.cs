using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// Interface for XFInAppBilling
    /// </summary>
    [Preserve(AllMembers = true)]
    public interface IXFInAppBilling: IDisposable
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
        /// <param name="verifyPurchase">only used for Google and IOS, not used for UWP, Amazon</param>
        /// <returns></returns>
        Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, IInAppBillingVerifyPurchase verifyPurchase = null);

        /// <summary>
        /// Gets all current purchases with status information
        /// </summary>
        /// <param name="itemType">only used for Google, not used for UWP, Amazon</param>    
        /// <param name="verifyPurchase">ios only</param>
        /// <param name="verifyOnlyProductId">ios only</param>
        /// <returns></returns>
        Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType = ItemType.InAppPurchase, IInAppBillingVerifyPurchase verifyPurchase = null, string verifyOnlyProductId = null);

        /// <summary>
        /// Checks if user has any active subscription. It mostly calls GetPurchasesAsync and filters by given subscriptionId
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
        Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType = ItemType.InAppPurchase);
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

        #region Apple

        /// <summary>
        /// Consume a purchase with a purchase token.
        /// </summary>
        /// <param name="productId">Id or Sku of product</param>
        /// <param name="purchaseToken">Original Purchase Token</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken);

        /// <summary>
        /// Consume a purchase
        /// </summary>
        /// <param name="productId">Id/Sku of the product</param>
        /// <param name="payload">Developer specific payload of original purchase</param>
        /// <param name="itemType">Type of product being consumed.</param>
        /// <param name="verifyPurchase">Verify Purchase implementation</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null);

        Task<bool> FinishTransaction(PurchaseResult purchase);

        Task<bool> FinishTransaction(string purchaseId);

        /// <summary>
        /// Verifies a specific product type and product id. Use e.g. when product is already purchased but verification failed and needs to be called again.
        /// </summary>
        /// <param name="itemType">Type of product</param>
        /// <param name="verifyPurchase">Interface to verify purchase</param>
        /// <param name="productId">Id of product</param>
        /// <returns>The current purchases</returns>
        Task<bool> VerifyPreviousPurchaseAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string productId);

        #endregion
    }
}
