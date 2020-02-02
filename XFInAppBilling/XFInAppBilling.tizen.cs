using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// UWP implementation
    /// </summary>
    public class XFInAppBillingImplementation : IXFInAppBilling
    {
        public Task<bool> CheckIfUserHasActiveSubscriptionAsync(string subscriptionId, ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            throw new NotImplementedException();
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinishTransaction(PurchaseResult purchase)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinishTransaction(string purchaseId)
        {
            throw new NotImplementedException();
        }

        public Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }

        public Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }

        public Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType = ItemType.InAppPurchase, IInAppBillingVerifyPurchase verifyPurchase = null, string verifyOnlyProductId = null)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyPreviousPurchaseAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string productId)
        {
            throw new NotImplementedException();
        }
    }
}
