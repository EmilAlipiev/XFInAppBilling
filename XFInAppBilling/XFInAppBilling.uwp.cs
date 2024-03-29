﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.Services.Store;

namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// UWP implementation
    /// </summary>
    public class XFInAppBillingImplementation : IXFInAppBilling, IDisposable
    {
        private StoreContext context = null;
        StoreProduct storeProduct;

        /// <summary>
        /// Constructor
        /// </summary>
        public XFInAppBillingImplementation()
        {
            Dispose(false);
        }

        #region API Functions
        /// <summary>
        /// Simulates connection. not used for UWP
        /// </summary>
        /// <returns></returns>
        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(true);
        }
        /// <summary>
        /// gets Iaps and Subs 
        /// </summary>
        /// <param name="ProductIds"></param>
        /// <param name="itemType">not used for UWP</param>
        /// <returns></returns>
        public async Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase)
        {

            var Products = new List<InAppBillingProduct>();
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // Subscription add-ons are Durable products.
            string[] productKinds = { "Durable", "Subscription" };
            var filterList = new List<string>(productKinds);

            StoreProductQueryResult queryResult =
                await context.GetAssociatedStoreProductsAsync(productKinds);

            if (queryResult.ExtendedError != null)
            {
                // The user may be offline or there might be some other server failure.
                throw new Exception(queryResult.ExtendedError.Message, queryResult.ExtendedError.InnerException);
            }

            if (queryResult?.Products?.Count > 0)
            {
                foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                {
                    // Access the Store product info for the add-on.
                    if (item.Value != null)
                    {
                        StoreProduct product = item.Value;

                        if (product.Skus != null)
                        {
                            // For each add-on, the subscription info is available in the SKU objects in the add-on. 
                            foreach (StoreSku sku in product.Skus)
                            {
                                if (sku.IsSubscription && sku.SubscriptionInfo != null)
                                {
                                    // Use the sku.SubscriptionInfo property to get info about the subscription. 
                                    // For example, the following code gets the units and duration of the 
                                    // subscription billing period.
                                    StoreDurationUnit billingPeriodUnit = sku.SubscriptionInfo.BillingPeriodUnit;
                                    uint billingPeriod = sku.SubscriptionInfo.BillingPeriod;

                                    Products.Add(new InAppBillingProduct()
                                    {
                                        ProductId = product.InAppOfferToken,
                                        LocalizedPrice = product.Price.FormattedRecurrencePrice,
                                        Description = sku.Description,
                                        Name = product.Title,
                                        OriginalPrice = product.Price.FormattedBasePrice,
                                        LocalizedIntroductoryPrice = product.Price.FormattedPrice,
                                        SaleEndDate = product.Price.SaleEndDate,
                                        IsOnSale = product.Price.IsOnSale,
                                        FreeTrialPeriod = sku.SubscriptionInfo.HasTrialPeriod ? sku.SubscriptionInfo.TrialPeriod + " " + sku.SubscriptionInfo.TrialPeriodUnit.ToString() : null
                                    });
                                }
                                else if (ProductIds?.Count > 0 && ProductIds.Contains(product.InAppOfferToken))
                                {
                                    Products.Add(new InAppBillingProduct()
                                    {
                                        ProductId = product.InAppOfferToken,
                                        LocalizedPrice = product.Price.FormattedPrice,
                                        OriginalPrice = product.Price.FormattedBasePrice,
                                        LocalizedIntroductoryPrice = product.Price.FormattedPrice,
                                        Description = sku.Description,
                                        Name = product.Title,
                                        SaleEndDate = product.Price.SaleEndDate,
                                        IsOnSale = product.Price.IsOnSale
                                    });
                                }

                            }
                        }
                    }
                }
            }

            return Products;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionStoreId"></param>
        /// <param name="itemType"></param>
        /// <param name="obfuscatedAccountId">not used for UWP</param>
        /// <param name="obfuscatedProfileId">not used for UWP</param>
        /// <returns></returns>
        public Task<PurchaseResult> PurchaseAsync(string subscriptionStoreId, ItemType itemType = ItemType.InAppPurchase, string obfuscatedAccountId = null, string obfuscatedProfileId = null)
        {
            return SetupSubscriptionInfoAsync(subscriptionStoreId);
        }

        /// <summary>
        /// Checks if user has active subscription and durables. it doesnt include consumeables
        /// </summary>
        /// <param name="subscriptionStoreId">if it is not provided, checks if there is an active licence.</param>
        /// <returns></returns>
        public async Task<bool> CheckIfUserHasActiveSubscriptionAsync(string subscriptionStoreId, ItemType itemType = ItemType.InAppPurchase)
        {
            if (context == null)
                context = StoreContext.GetDefault();
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();

            return appLicense.AddOnLicenses.Any(s => (s.Value.InAppOfferToken.StartsWith("sub_") || s.Value.SkuStoreId.StartsWith("sub_"))
             && s.Value.ExpirationDate > DateTime.Now);

        }
        /// <summary>
        /// Get All Purchases
        /// </summary>
        /// <param name="itemType">not used for UWP</param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType = ItemType.InAppPurchase, List<string> doNotFinishTransactionIds = null)
        {
            if (context == null)
                context = StoreContext.GetDefault();
            var PurchaseHistoryResult = new List<PurchaseResult>();
            StoreAppLicense appLicense = await context.GetAppLicenseAsync();

            if (appLicense?.AddOnLicenses?.Count > 0)
            {
                foreach (var addOnLicense in appLicense.AddOnLicenses)
                {
                    StoreLicense license = addOnLicense.Value;
                    var purchaseHistory = new PurchaseResult();
                    purchaseHistory.Sku = license.InAppOfferToken; //UWP SkuStoreId is different than Product ID, InAppOfferToken is the product ID
                    purchaseHistory.PurchaseToken = license.SkuStoreId;

                    purchaseHistory.ExpirationDate = license.ExpirationDate;
                    if (!license.IsActive)
                        purchaseHistory.PurchaseState = PurchaseState.Cancelled;
                    else
                        purchaseHistory.PurchaseState = PurchaseState.Purchased;

                    PurchaseHistoryResult.Add(purchaseHistory);
                }
            }

            // The customer does not have a license to the subscription.
            return PurchaseHistoryResult;
        }

        /// <summary>
        /// Gets all the purchased products for current user and app
        /// </summary>
        /// <param name="itemType">not used for UWP</param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType = ItemType.InAppPurchase)
        {

            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // Specify the kinds of add-ons to retrieve.
            string[] productKinds = { "Durable", "Subscription" };
            var filterList = new List<string>(productKinds);

            StoreProductQueryResult queryResult = await context.GetUserCollectionAsync(filterList);

            if (queryResult.ExtendedError != null)
            {
                // The user may be offline or there might be some other server failure.
                throw new Exception(queryResult.ExtendedError.Message, queryResult.ExtendedError.InnerException);
            }
            var purchaseHistoryResult = new List<PurchaseResult>();
            if (queryResult?.Products?.Count > 0)
            {
                foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                {
                    StoreProduct product = item.Value;

                    var purchaseHistory = new PurchaseResult();

                    purchaseHistory.Sku = product.InAppOfferToken;
                    purchaseHistory.PurchaseToken = null;

                    purchaseHistory.DeveloperPayload = product.Skus?[0].CustomDeveloperData;

                    purchaseHistoryResult.Add(purchaseHistory);
                    // Use members of the product object to access info for the product...

                }
            }

            return purchaseHistoryResult;
        }

        /// <summary>
        /// Disconnects iap instance
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            return true;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriptionStoreId"></param>
        /// <returns></returns>
        private async Task<PurchaseResult> SetupSubscriptionInfoAsync(string subscriptionStoreId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            bool userOwnsSubscription = await CheckIfUserHasActiveSubscriptionAsync(subscriptionStoreId);
            if (userOwnsSubscription)
            {
                // Unlock all the subscription add-on features here.
                return new PurchaseResult() { PurchaseState = PurchaseState.Purchased, Sku = subscriptionStoreId };
            }

            // Get the StoreProduct that represents the subscription add-on.
            await GetSubscriptionProductAsync(subscriptionStoreId);
            if (storeProduct == null)
            {
                return new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = subscriptionStoreId };
            }

            // Check if the first SKU is a trial and notify the customer that a trial is available.
            // If a trial is available, the Skus array will always have 2 purchasable SKUs and the
            // first one is the trial. Otherwise, this array will only have one SKU.
            StoreSku sku = storeProduct.Skus[0];
            //if (sku.SubscriptionInfo.HasTrialPeriod)
            //{
            //    // You can display the subscription trial info to the customer here. You can use 
            //    // sku.SubscriptionInfo.TrialPeriod and sku.SubscriptionInfo.TrialPeriodUnit 
            //    // to get the trial details.
            //}
            //else
            //{
            //    // You can display the subscription purchase info to the customer here. You can use 
            //    // sku.SubscriptionInfo.BillingPeriod and sku.SubscriptionInfo.BillingPeriodUnit
            //    // to provide the renewal details.
            //}

            // Prompt the customer to purchase the subscription.
            return await PromptUserToPurchaseAsync(sku);
        }

        private async Task<InAppBillingProduct> GetSubscriptionProductAsync(string subscriptionStoreId)
        {
            var Products = new List<InAppBillingProduct>();
            // Load the sellable add-ons for this app and check if the trial is still 
            // available for this customer. If they previously acquired a trial they won't 
            // be able to get a trial again, and the StoreProduct.Skus property will 
            // only contain one SKU.
            StoreProductQueryResult result =
                await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

            if (result.ExtendedError != null)
            {
                throw new Exception(result.ExtendedError.Message, result.ExtendedError.InnerException);
            }

            // Look for the product that represents the subscription.
            foreach (var item in result.Products)
            {
                StoreProduct product = item.Value;
                if (product.InAppOfferToken == subscriptionStoreId)
                {
                    var sku = product.Skus[0];
                    storeProduct = product;
                    return new InAppBillingProduct()
                    {
                        ProductId = product.InAppOfferToken,
                        LocalizedPrice = product.Price.FormattedPrice,
                        Description = product.Description,
                        Name = product.Title
                    };
                }
            }


            return null;
        }

        /// <summary>
        /// Prompt for purchase popup
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        private async Task<PurchaseResult> PromptUserToPurchaseAsync(StoreSku sku)
        {
            string productId = sku.StoreId;
            // Request a purchase of the subscription product. If a trial is available it will be offered 
            // to the customer. Otherwise, the non-trial SKU will be offered.
            StorePurchaseResult result = await storeProduct.RequestPurchaseAsync();

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            return result.Status switch
            {
                StorePurchaseStatus.Succeeded => new PurchaseResult() { PurchaseState = PurchaseState.Purchased, Sku = productId },// Show a UI to acknowledge that the customer has purchased your subscription 
                                                                                                                                   // and unlock the features of the subscription. 
                StorePurchaseStatus.NotPurchased => new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = productId },
                StorePurchaseStatus.ServerError => new PurchaseResult() { PurchaseState = PurchaseState.ServerError, Sku = productId },
                StorePurchaseStatus.NetworkError => new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = productId },
                StorePurchaseStatus.AlreadyPurchased => new PurchaseResult() { PurchaseState = PurchaseState.Purchased, Sku = productId },
                _ => new PurchaseResult() { PurchaseState = PurchaseState.Unknown, Sku = productId },
            };
        }

        /// <summary>
        /// Consumes a consumable iap
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="purchaseToken"></param>
        /// <returns></returns>
        public async Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken = null)
        {
            return await ConsumePurchase(productId);
        }

        private async Task<PurchaseResult> ConsumePurchase(string productId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // This is an example for a Store-managed consumable, where you specify the actual number
            // of units that you want to report as consumed so the Store can update the remaining
            // balance. For a developer-managed consumable where you maintain the balance, specify 1
            // to just report the add-on as fulfilled to the Store.
            uint quantity = 1;
            string addOnStoreId = productId;

            var trackingId = Guid.NewGuid();

            StoreConsumableResult result = await context.ReportConsumableFulfillmentAsync(
                addOnStoreId, quantity, trackingId);

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            return result.Status switch
            {
                StoreConsumableStatus.Succeeded => new PurchaseResult() { PurchaseState = PurchaseState.Purchased, Sku = productId },// Show a UI to acknowledge that the customer has purchased your subscription                                                                                                                   // and unlock the features of the subscription. 
                StoreConsumableStatus.InsufficentQuantity => new PurchaseResult() { PurchaseState = PurchaseState.InsufficentQuantity, Sku = productId },
                StoreConsumableStatus.ServerError => new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = productId },
                StoreConsumableStatus.NetworkError => new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = productId },
                _ => new PurchaseResult() { PurchaseState = PurchaseState.Failed, Sku = productId },
            };
        }

        #region NOTUSED FOR UWP

        public string ReceiptData { get; }
        /// <summary>
        ///  IOS only, not implemented for Android
        /// </summary>
        /// <param name="purchase"></param>
        /// <returns></returns>
        public Task<bool> FinishTransaction(PurchaseResult purchase)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  IOS only, not implemented for Android
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        public Task<bool> FinishTransaction(string purchaseId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  IOS only, not implemented for Android
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="verifyPurchase"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public Task<bool> VerifyPreviousPurchaseAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase, string productId)
        {
            throw new NotImplementedException();
        }

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
