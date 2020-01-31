using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// Base implementation for In App Billing, handling disposables
    /// </summary>
    public abstract class BaseInAppBilling : IXFInAppBilling, IDisposable
    {
        /// <summary>
        /// Gets or sets if in testing mode
        /// </summary>
        public abstract bool InTestingMode { get; set; }


        /// <summary>
        /// Connect to billing service
        /// </summary>
        /// <returns>If Success</returns>
        public abstract Task<bool> ConnectAsync();


        /// <summary>
        /// Disconnect from the billing service
        /// </summary>
        /// <returns>Task to disconnect</returns>
        public abstract bool Disconnect();

        /// <summary>
        /// Get product information of a specific product
        /// </summary>
        /// <param name="ProductIds">Sku or Id of the product(s)</param>
        /// <param name="itemType">Type of product offering</param>
        /// <returns>List of InAppBillingProduct</returns>
        public abstract Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase);
         

		/// <summary>
		/// Verifies a specific product type and product id. Use e.g. when product is already purchased but verification failed and needs to be called again.
		/// </summary>
		/// <param name="itemType">Type of product</param>
		/// <param name="verifyPurchase">Interface to verify purchase</param>
		/// <param name="productId">Id of product</param>
		/// <returns>The current purchases</returns>
		public async Task<bool> VerifyPreviousPurchaseAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string productId)
		{
			return (await GetPurchasesAsync(itemType, verifyPurchase, productId))?.Any(p => productId.Equals(p?.Sku)) ?? false;
		}

		/// <summary>
		/// Get all current purchases for a specific product type. If verification fails for some purchase, it's not contained in the result.
		/// </summary>
		/// <param name="itemType">Type of product</param>
		/// <param name="verifyPurchase">Interface to verify purchase</param>
		/// <param name="verifyOnlyProductId">If you want to verify a specific purchase, provide its id. Other purchases will be returned without verification.</param>
		/// <returns>The current purchases</returns>
		protected abstract Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string verifyOnlyProductId);

		/// <summary>
		/// Get all current purchases for a specific product type. If you use verification and it fails for some purchase, it's not contained in the result.
		/// </summary>
		/// <param name="itemType">Type of product</param>
        /// <param name="verifyPurchase">Verify purchase implementation</param>
		/// <returns>The current purchases</returns>
		public async Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase = null)
		{
			return await GetPurchasesAsync(itemType, verifyPurchase, null);
		}
      
        /// <summary>
        /// Purchase a specific product or subscription
        /// </summary>
        /// <param name="productId">Sku or ID of product</param>
        /// <param name="itemType">Type of product being requested</param>
        /// <param name="payload">Developer specific payload</param>
        /// <param name="verifyPurchase">Verify Purchase implementation</param>
        /// <returns>Purchase details</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        public abstract Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, IInAppBillingVerifyPurchase verifyPurchase = null);

        /// <summary>
        /// Consume a purchase with a purchase token.
        /// </summary>
        /// <param name="productId">Id or Sku of product</param>
        /// <param name="purchaseToken">Original Purchase Token</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        public abstract Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken);

        /// <summary>
        /// Consume a purchase
        /// </summary>
        /// <param name="productId">Id/Sku of the product</param>
        /// <param name="payload">Developer specific payload of original purchase</param>
        /// <param name="itemType">Type of product being consumed.</param>
        /// <param name="verifyPurchase">Verify Purchase implementation</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        public abstract Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null);

        /// <summary>
        /// Dispose of class and parent classes
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose up
        /// </summary>
        ~BaseInAppBilling()
        {
            Dispose(false);
        }

        private bool disposed = false;
        /// <summary>
        /// Dispose method
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    //dispose only
                }

                disposed = true;
            }
        }

		public virtual Task<bool> FinishTransaction(PurchaseResult purchase) => Task.FromResult(true);

		public virtual Task<bool> FinishTransaction(string purchaseId) => Task.FromResult(true);

        public abstract Task<bool> CheckIfUserHasActiveSubscriptionAsync(string subscriptionId, ItemType itemType = ItemType.InAppPurchase);       

        public Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }

    
    }
}
