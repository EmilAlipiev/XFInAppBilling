using com.amazon.device.iap.cpt;

using Plugin.XFInAppBilling;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

using XFInAppBilling.Tests.Amazon.IAP;

[assembly: Dependency(typeof(InAppBillingImplementation))]
namespace XFInAppBilling.Tests.Amazon.IAP
{
    public class InAppBillingImplementation : IXFInAppBilling
    {
        private IAmazonIapV2 context;
        public string ReceiptData { get; }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        #region API Functions
        /// <summary>
        /// Simulates connection. not used for Amazon
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// get iaps and subs information by skus
        /// </summary>
        /// <param name="ProductIds">list of iaps to be retrieved. iap and subs are mixed</param>
        /// <param name="itemType">not required for amazon</param>
        /// <returns></returns>
        public async Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase)
        {
            var taskCompletionSource = new TaskCompletionSource<List<InAppBillingProduct>>();


            //await Task.Delay(TimeSpan.FromMilliseconds(1));an.FromMilliseconds(1));
            if (context == null)
                context = AmazonIapV2Impl.Instance;


            // Construct object passed to operation as input
            SkusInput request = new SkusInput();
            request.Skus = ProductIds;

            // Call synchronous operation with input object
            string requestId = context.GetProductData(request).RequestId;
            // Get return value
            GetProductDataResponseDelegator delegator = null;

            delegator = new GetProductDataResponseDelegator(async response =>
            {
                await Task.Run(() =>
                {
                    if (response.RequestId == requestId)
                    {
                        var result = GetProductEventHandler(response);
                        var sucess = taskCompletionSource.TrySetResult(result);

                        context.RemoveGetProductDataResponseListener(delegator.responseDelegate);
                    }
                });

            });

            // Register for an event
            context.AddGetProductDataResponseListener(delegator.responseDelegate);

            return await taskCompletionSource.Task;

        }

        /// <summary>
        /// Purchase iap or sub
        /// </summary>
        /// <param name="productId">iap or sub sku to be purchased</param>
        /// <param name="itemType">not required for amazon</param>
        /// <param name="payload">not used for amazon</param>
        /// <param name="verifyPurchase">not used for amazon</param>
        /// <returns></returns>
        public async Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string obfuscatedAccountId = null, string? obfuscatedProfileId = null, string subOfferToken = null)
        {

            if (await CheckIfUserHasActiveSubscriptionAsync(productId))
                return new PurchaseResult() { PurchaseState = PurchaseState.Purchased, ProductId = productId };

            if (context == null)
                context = AmazonIapV2Impl.Instance;


            //bool userOwnsSubscription = await CheckIfUserHasSubscriptionAsync(subscriptionStoreId);
            //if (userOwnsSubscription)
            //{
            //    Settings.IsSubscribed = true;

            //    // Unlock all the subscription add-on features here.
            //    return true;
            //}
            // Construct object passed to operation as input
            //await Task.Delay(TimeSpan.FromMilliseconds(1));an.FromMilliseconds(1));
            var taskCompletionSource = new TaskCompletionSource<PurchaseResult>();

            try
            {
                SkuInput request = new SkuInput();

                // Set input value
                request.Sku = productId;

                // Call synchronous operation with input object
                string requestId = context.Purchase(request).RequestId;
                // Get return value

                PurchaseResponseDelegator delegator = null;
                delegator = new PurchaseResponseDelegator(async response =>
                {
                    await Task.Run(() =>
                    {
                        if (response.RequestId == requestId)
                        {
                            var result = GetPurchaseEventHandler(response);
                            var sucess = taskCompletionSource.TrySetResult(result);
                            //await Task.Delay(TimeSpan.FromMilliseconds(1));
                            context.RemovePurchaseResponseListener(delegator.responseDelegate);
                        }
                    });
                });
                // Register for an event
                context.AddPurchaseResponseListener(delegator.responseDelegate);

                return await taskCompletionSource.Task;
            }
            catch (Exception)
            {
                return new PurchaseResult() { PurchaseState = PurchaseState.Failed, ProductId = productId };
            }

        }

        /// <summary>
        /// returns all purchases
        /// </summary>
        /// <param name="itemType">not used for Amazon</param>
        /// <param name="verifyPurchase">Not Used for Amazon</param>
        /// <param name="verifyOnlyProductId">Not Used for Amazon</param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType = ItemType.InAppPurchase, List<string> doNotFinishTransactionIds = null)
        {
            var purchaseHistoryResult = new List<PurchaseResult>();
            var purchaseReceipts = await GetPurchaseReceipts();
            if (purchaseReceipts?.Count > 0)
            {
                foreach (var purchase in purchaseReceipts)
                {
                    var purchaseHistory = new PurchaseResult
                    {
                        ProductId = purchase.Sku,
                        PurchaseToken = purchase.ReceiptId
                    };

                    if (purchase.PurchaseDate > 0)
                        purchaseHistory.PurchaseDate = DateTimeOffset.FromUnixTimeSeconds(purchase.PurchaseDate).DateTime;

                    purchaseHistory.DeveloperPayload = null;
                    if (purchase.CancelDate > 0)
                    {
                        purchaseHistory.ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(purchase.CancelDate).DateTime;
                        purchaseHistory.PurchaseState = PurchaseState.Cancelled;

                    }
                    else
                        purchaseHistory.PurchaseState = PurchaseState.Purchased;

                    purchaseHistoryResult.Add(purchaseHistory);
                }
            }

            return purchaseHistoryResult;
        }

        /// <summary>
        /// Checks if user has a valid subscription
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfUserHasActiveSubscriptionAsync(string productId, ItemType itemType = ItemType.InAppPurchase)
        {

            //await Task.Delay(TimeSpan.FromMilliseconds(1));an.FromMilliseconds(1));

            var receipts = await GetPurchaseReceipts();

            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            bool found = false;
            if (receipts?.Count > 0)
            {  //TODO: verify here if can be done with any subscriptionStoreId
                if (productId != null)
                    found = receipts.Any(r => r.ProductType.ToUpper() == "SUBSCRIPTION" && r.CancelDate > 0 && !(start.AddMilliseconds(r.CancelDate).ToUniversalTime() <= DateTime.UtcNow));
                else
                    found = true;
            }

            return found;

        }

        public bool Disconnect()
        {
            return true;
        }

        /// <summary>
        ///  initiates a request to retrieve updates about items the customer has purchased and/or cancelled.
        /// </summary>
        /// <param name="itemType">not required for Amazon</param>
        /// <returns></returns>
        private async Task<List<PurchaseReceipt>> GetPurchaseReceipts()
        {
            var taskCompletionSource = new TaskCompletionSource<List<PurchaseReceipt>>();

            if (context == null)
                context = AmazonIapV2Impl.Instance;


            // Construct object passed to operation as input
            var request = new ResetInput();

            // Set input value
            request.Reset = true;

            // Call synchronous operation with input object
            string requestId = context.GetPurchaseUpdates(request).RequestId;
            // Get return value
            GetPurchaseUpdatesResponseDelegator delegator = null;
            delegator = new GetPurchaseUpdatesResponseDelegator(async response =>
            {
                await Task.Run(() =>
                {
                    if (response.RequestId == requestId)
                    {
                        var receipts = GetPurchasesUpdateEventHandler(response);

                        var sucess = taskCompletionSource.TrySetResult(receipts);
                        //await Task.Delay(TimeSpan.FromMilliseconds(1));an.FromMilliseconds(1));
                        context.RemoveGetPurchaseUpdatesResponseListener(delegator.responseDelegate);
                    }
                });
            });
            // Register for an event
            context.AddGetPurchaseUpdatesResponseListener(delegator.responseDelegate);

            return await taskCompletionSource.Task;
        }

        /// <summary>
        /// Notifies fullfillment after a purchase
        /// </summary>
        /// <param name="receiptID"></param>
        /// <returns></returns>
        private bool NotifyFullFillment(string receiptID)
        {

            IAmazonIapV2 iapService = AmazonIapV2Impl.Instance;
            // Construct object passed to operation as input
            NotifyFulfillmentInput request = new NotifyFulfillmentInput();

            // Set input values
            request.ReceiptId = receiptID;
            request.FulfillmentResult = "FULFILLED";

            // Call synchronous operation with input object
            iapService.NotifyFulfillment(request);


            return true;
        }
        #endregion

        #region Response Handlers
        private List<PurchaseReceipt> GetPurchasesUpdateEventHandler(GetPurchaseUpdatesResponse args)
        {
            if (args == null || args.Receipts == null || args.AmazonUserData == null)
                return null;

            string requestId = args.RequestId;
            string userId = args.AmazonUserData.UserId;
            string marketplace = args.AmazonUserData.Marketplace;
            List<PurchaseReceipt> receipts = args.Receipts;
            string status = args.Status;
            bool hasMore = args.HasMore;

            //// for each purchase receipt you can get the following values
            //string receiptId = receipts[0].ReceiptId;
            //long cancelDate = receipts[0].CancelDate;
            //long purchaseDate = receipts[0].PurchaseDate;
            //string sku = receipts[0].ProductId;
            //string productType = receipts[0].ProductType;

            return receipts;
        }

        // Define event handler
        private void GetUserDataEventHandler(GetUserDataResponse args)
        {
            string requestId = args.RequestId;
            string userId = args.AmazonUserData.UserId;
            string marketplace = args.AmazonUserData.Marketplace;
            string status = args.Status;
        }

        private PurchaseResult GetPurchaseEventHandler(PurchaseResponse args)
        {
            var purchaseResult = new PurchaseResult();

            if (args?.PurchaseReceipt == null || args.AmazonUserData == null)
                return purchaseResult;


            purchaseResult.OrderId = args.RequestId;
            purchaseResult.UserId = args.AmazonUserData.UserId;
            string marketplace = args.AmazonUserData.Marketplace;
            purchaseResult.PurchaseToken = args.PurchaseReceipt.ReceiptId;
            if (args.PurchaseReceipt.CancelDate > 0)
                purchaseResult.ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(args.PurchaseReceipt.CancelDate).DateTime;
            if (args.PurchaseReceipt.PurchaseDate > 0)
                purchaseResult.PurchaseDate = DateTimeOffset.FromUnixTimeSeconds(args.PurchaseReceipt.PurchaseDate).DateTime;
            purchaseResult.ProductId = args.PurchaseReceipt.Sku;
            purchaseResult.ItemType = args.PurchaseReceipt.ProductType;
            if (args.Status == "Cancelled")
                purchaseResult.PurchaseState = PurchaseState.Cancelled;
            else
                purchaseResult.PurchaseState = PurchaseState.Purchased;

            if (NotifyFullFillment(args.PurchaseReceipt.ReceiptId))
                purchaseResult.IsAcknowledged = true;

            return purchaseResult;

        }

        // Define event handler
        private List<InAppBillingProduct> GetProductEventHandler(GetProductDataResponse args)
        {
            if (args == null)
                return null;

            string requestId = args.RequestId;
            List<InAppBillingProduct> Products = new List<InAppBillingProduct>();
            Dictionary<string, ProductData> IapProducts = args.ProductDataMap;
            // List<string> unavailableSkus = args.UnavailableSkus;
            if (IapProducts?.Count > 0)
            {
                foreach (var Product in IapProducts)
                {
                    Products.Add(new InAppBillingProduct
                    {
                        Description = Product.Value.Description,
                        LocalizedPrice = Product.Value.Price,
                        ProductId = Product.Value.Sku,
                        Name = Product.Value.Title
                    });
                }
            }

            return Products;
            //string status = args.Status;

            //// for each item in the productDataMap you can get the following values for a given SKU
            //// (replace "sku" with the actual SKU)
            //string sku = Products["sku"].ProductId;
            //string productType = Products["sku"].ProductType;
            //string price = Products["sku"].Price;
            //string title = Products["sku"].Title;
            //string description = Products["sku"].Description;
            //string smallIconUrl = Products["sku"].SmallIconUrl;
        }

        /// <summary>
        /// It returns GetPurchases. Amazon doesn't separate active or all purchases. it has all purchases in 1 function with statuses
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType = ItemType.InAppPurchase)
        {
            return await GetPurchasesAsync(itemType);
        }
        #endregion

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            throw new NotImplementedException();
        }
        #region NOTUSED FOR AMAZON

        public Task<bool> FinishTransaction(PurchaseResult purchase)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinishTransaction(string purchaseId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyPreviousPurchaseAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string productId)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> UpdateSubscriptionAsync(string oldSubscriptionToken, string newSubscriptionId, Proration proration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AcknowledgePurchase(string purchaseToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}