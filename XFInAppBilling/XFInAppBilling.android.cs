using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.BillingClient.Api;
using Android.Content;
using Plugin.CurrentActivity;


namespace Plugin.XFInAppBilling
{
    [Preserve(AllMembers = true)]
    public class XFInAppBillingImplementation : Java.Lang.Object, IXFInAppBilling, IBillingClientStateListener, ISkuDetailsResponseListener, IPurchasesUpdatedListener, IAcknowledgePurchaseResponseListener, IPurchaseHistoryResponseListener
    {
        private const string TAG = "BillingManager";
        public const int BILLING_MANAGER_NOT_INITIALIZED = -1;
        private bool IsServiceConnected;

        private Context CurrentContext => CrossCurrentActivity.Current.Activity;

        public BillingClient BillingClient { get; set; }

        // private PurchaseResult PurchaseResult { get; set; } = new PurchaseResult();
        private List<PurchaseResult> PurchaseHistoryResult { get; set; } = new List<PurchaseResult>();

        private List<InAppBillingProduct> InAppBillingProducts { get; set; } = new List<InAppBillingProduct>();

        TaskCompletionSource<bool> tcsConnect;
        TaskCompletionSource<PurchaseResult> tcsPurchase;
        TaskCompletionSource<List<InAppBillingProduct>> tcsProducts;
        TaskCompletionSource<List<PurchaseResult>> tcsPurchaseHistory;
        TaskCompletionSource<bool> tcsAcknowledge;

        public async Task<bool> ConnectAsync()
        {
            tcsConnect = new TaskCompletionSource<bool>();

            BillingClient = BillingClient.NewBuilder(CurrentContext).EnablePendingPurchases().SetListener(this).Build();

            BillingClient.StartConnection(this);

            return await tcsConnect?.Task;

        }


        #region API Functions

        public async Task<List<InAppBillingProduct>> GetProductsAsync(List<string> ProductIds, ItemType itemType = ItemType.InAppPurchase)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            tcsProducts = new TaskCompletionSource<List<InAppBillingProduct>>();
            var prms = SkuDetailsParams.NewBuilder();
            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            prms.SetSkusList(ProductIds).SetType(type);

            BillingClient.QuerySkuDetailsAsync(prms.Build(), this);

            return await tcsProducts?.Task ?? default;


        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disconnect();
        }

        /// <summary>
        /// Check if user has the subscription or in app item purchased
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfUserHasActiveSubscriptionAsync(string productId, ItemType itemType = ItemType.InAppPurchase)
        {
            var purchases = await GetPurchasesAsync(itemType);

            bool found = false;
            if (purchases?.Count > 0)
            {
                if (itemType == ItemType.Subscription)
                    found = purchases.Any(r => r.IsAcknowledged);
                else
                    found = purchases.Any(r => r.IsAcknowledged && r.Sku == productId);

            }

            return found;
        }

        /// <summary>
        /// Get Purchases with the status information
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchaseHistoryAsync(ItemType itemType)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            tcsPurchaseHistory = new TaskCompletionSource<List<PurchaseResult>>();
 
            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            BillingClient.QueryPurchaseHistoryAsync(type, this);

            return await tcsPurchaseHistory?.Task ?? default;
        }

        /// <summary>
        /// Get All purchases regardless of status
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType, IInAppBillingVerifyPurchase verifyPurchase = null, string verifyOnlyProductId = null)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            var prms = SkuDetailsParams.NewBuilder();
            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            var purchaseResult = BillingClient.QueryPurchases(type);
            var purchases = await GetPurchasesAsync(purchaseResult.PurchasesList);

            return purchases;

        }

        private SkuDetails ProductToPurcase { get; set; }
        public async Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            var purchaseResult = new PurchaseResult();
            var productIds = new List<string>();
            productIds.Add(productId);

            await GetProductsAsync(productIds, itemType);

            if (ProductToPurcase != null)
            {
                purchaseResult = await DoPurchaseAsync(ProductToPurcase);
            }

            return purchaseResult;

        }

        public bool Disconnect()
        {
            if (BillingClient != null)
            {
                BillingClient.EndConnection();
                BillingClient = null;
            }

            return true;
        }

        /// <summary>
        /// If you use the Google Play Billing Library version 2.0 or newer, you must acknowledge all purchases within three days.
        /// </summary>
        /// <param name="receiptID"></param>
        /// <returns></returns>
        private async Task<bool> NotifyFullFillmentAsync(Purchase purchase)
        {
            if (!purchase.IsAcknowledged)
            {
                if (BillingClient == null || !BillingClient.IsReady)
                {
                    await ConnectAsync();
                }

                tcsAcknowledge = new TaskCompletionSource<bool>();

                AcknowledgePurchaseParams acknowledgePurchaseParams =
                                        AcknowledgePurchaseParams.NewBuilder()
                                            .SetPurchaseToken(purchase.PurchaseToken)
                                            .Build();
                BillingClient.AcknowledgePurchase(acknowledgePurchaseParams, this);

                return await tcsAcknowledge?.Task;
            }
            return true;

        }

        #endregion

        private async Task<PurchaseResult> DoPurchaseAsync(SkuDetails product)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }
            tcsPurchase = new TaskCompletionSource<PurchaseResult>();

            BillingFlowParams flowParams = BillingFlowParams.NewBuilder().SetSkuDetails(product).Build();
            BillingResult responseCode = BillingClient.LaunchBillingFlow(CrossCurrentActivity.Current.Activity, flowParams);
            return await tcsPurchase?.Task ?? default;
        }

        #region ResponseHandlers

        public void OnPurchaseHistoryResponse(BillingResult billingResult, IList<PurchaseHistoryRecord> purchases)
        {
            PurchaseHistoryResult = new List<PurchaseResult>();
            if (billingResult.ResponseCode == BillingResponseCode.Ok && purchases != null)
            {
                foreach (var purchase in purchases)
                {
                    var purchaseHistory = new PurchaseResult();

                    purchaseHistory.Sku = purchase.Sku;
                    purchaseHistory.PurchaseToken = purchase.PurchaseToken;

                    if (purchase.PurchaseTime > 0)
                        purchaseHistory.PurchaseDate = DateTimeOffset.FromUnixTimeMilliseconds(purchase.PurchaseTime).DateTime;

                    purchaseHistory.DeveloperPayload = purchase.DeveloperPayload;

                    PurchaseHistoryResult.Add(purchaseHistory);
                }
            }

            GetResponseCode(billingResult.ResponseCode);
            tcsPurchaseHistory?.TrySetResult(PurchaseHistoryResult);
        }
        public void OnAcknowledgePurchaseResponse(BillingResult billingResult)
        {
            var isAcknowledged = billingResult.ResponseCode == BillingResponseCode.Ok;

            GetResponseCode(billingResult.ResponseCode);
            tcsAcknowledge?.TrySetResult(isAcknowledged);
        }

        public async void OnPurchasesUpdated(BillingResult billingResult, IList<Purchase> purchases)
        {
            var purchaseResult = new PurchaseResult();
            if (billingResult.ResponseCode == BillingResponseCode.Ok && purchases != null)
            {
                purchaseResult.PurchaseState = Plugin.XFInAppBilling.PurchaseState.Purchased;
                var purchaseResults = await GetPurchasesAsync(purchases);
                purchaseResult = purchaseResults?.OrderByDescending(p => p.ExpirationDate).Last();
            }
            else if (billingResult.ResponseCode == BillingResponseCode.UserCancelled)
            {
                purchaseResult.PurchaseState = Plugin.XFInAppBilling.PurchaseState.Cancelled;
            }
            else if (billingResult.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
            {
                purchaseResult.PurchaseState = Plugin.XFInAppBilling.PurchaseState.AlreadyOwned;
            }
            else
            {
                purchaseResult.PurchaseState = Plugin.XFInAppBilling.PurchaseState.Failed;
            }

            GetResponseCode(billingResult.ResponseCode);
            tcsPurchase?.TrySetResult(purchaseResult);

        }

        private async Task<List<PurchaseResult>> GetPurchasesAsync(IList<Purchase> purchases)
        {
            var purchaseResults = new List<PurchaseResult>();
            if (purchases?.Count > 0)
            {
                foreach (var purchase in purchases)
                {
                    var purchaseResult = new PurchaseResult();
                    Plugin.XFInAppBilling.PurchaseState purchaseState;
                    switch (purchase.PurchaseState)
                    {
                        case Android.BillingClient.Api.PurchaseState.Pending:
                            purchaseState = Plugin.XFInAppBilling.PurchaseState.Pending;
                            break;
                        case Android.BillingClient.Api.PurchaseState.Purchased:
                            // Acknowledge the purchase if it hasn't already been acknowledged.
                            if (await NotifyFullFillmentAsync(purchase))
                                purchaseState = Plugin.XFInAppBilling.PurchaseState.Purchased;
                            else
                                purchaseState = Plugin.XFInAppBilling.PurchaseState.NotAknowledged;
                            break;
                        case Android.BillingClient.Api.PurchaseState.Unspecified:
                            purchaseState = Plugin.XFInAppBilling.PurchaseState.Unspecified;
                            break;
                        default:
                            purchaseState = Plugin.XFInAppBilling.PurchaseState.Unspecified;
                            break;
                    }
                    purchaseResult.Sku = purchase.Sku;
                    purchaseResult.PurchaseToken = purchase.PurchaseToken;
                    purchaseResult.PurchaseState = purchaseState;
                    if (purchase.PurchaseTime > 0)
                        purchaseResult.PurchaseDate = DateTimeOffset.FromUnixTimeMilliseconds(purchase.PurchaseTime).DateTime;

                    purchaseResult.DeveloperPayload = purchase.DeveloperPayload;
                    purchaseResult.IsAcknowledged = purchase.IsAcknowledged;
                    purchaseResult.IsAutoRenewing = purchase.IsAutoRenewing;
                    purchaseResult.OrderId = purchase.OrderId;

                    purchaseResults.Add(purchaseResult);
                }

            }

            return purchaseResults;

        }

        public void OnSkuDetailsResponse(BillingResult billingResult, IList<SkuDetails> skuDetails)
        {
            InAppBillingProducts = new List<InAppBillingProduct>();
            if (billingResult.ResponseCode == BillingResponseCode.Ok)
            {

                // List<string> unavailableSkus = args.UnavailableSkus;
                if (skuDetails?.Count > 0)
                {
                    foreach (var product in skuDetails)
                    {
                        InAppBillingProducts.Add(new InAppBillingProduct
                        {
                            Description = product.Description,
                            LocalizedPrice = product.Price,
                            LocalizedIntroductoryPrice = product.IntroductoryPrice,
                            CurrencyCode = product.PriceCurrencyCode,
                            MicrosIntroductoryPrice = product.IntroductoryPriceAmountMicros,
                            MicrosPrice = product.PriceAmountMicros,
                            ProductId = product.Sku,
                            Name = product.Title,
                            Type = product.Type,
                            IconUrl = product.IconUrl,
                            IsRewarded = product.IsRewarded,
                            IntroductoryPrice = product.IntroductoryPrice,
                            IntroductoryPriceCycles = product.IntroductoryPriceCycles,
                            IntroductoryPricePeriod = product.IntroductoryPricePeriod,
                            SubscriptionPeriod = product.SubscriptionPeriod,
                            FreeTrialPeriod = product.FreeTrialPeriod,
                            OriginalPrice = product.OriginalPrice,
                            OriginalPriceAmountMicros = product.OriginalPriceAmountMicros
                        });
                    }

                    ProductToPurcase = skuDetails[0];
                }
            }

            GetResponseCode(billingResult.ResponseCode);
            tcsProducts?.TrySetResult(InAppBillingProducts);
        }
        public async void OnBillingServiceDisconnected()
        {
            await ConnectAsync();
            tcsConnect?.TrySetResult(false);
        }

        public void OnBillingSetupFinished(BillingResult billingResult)
        {
            IsServiceConnected = billingResult.ResponseCode == BillingResponseCode.Ok;

            var exception = GetResponseCode(billingResult.ResponseCode);
            if (exception == null)
                tcsConnect?.TrySetResult(IsServiceConnected);
            else
                tcsConnect?.SetException(exception);
        }
        #endregion
        private InAppBillingPurchaseException GetResponseCode(BillingResponseCode billingResponseCode)
        {
            switch (billingResponseCode)
            {
                case BillingResponseCode.BillingUnavailable:
                    return new InAppBillingPurchaseException(PurchaseError.BillingUnavailable);
                case BillingResponseCode.DeveloperError:
                    return new InAppBillingPurchaseException(PurchaseError.DeveloperError);
                case BillingResponseCode.Error:
                    return new InAppBillingPurchaseException(PurchaseError.GeneralError);
                case BillingResponseCode.FeatureNotSupported:
                    return new InAppBillingPurchaseException(PurchaseError.FeatureNotSupported);
                case BillingResponseCode.ItemAlreadyOwned:
                    return null;
                case BillingResponseCode.ItemNotOwned:
                    return new InAppBillingPurchaseException(PurchaseError.NotOwned);
                case BillingResponseCode.ItemUnavailable:
                    return new InAppBillingPurchaseException(PurchaseError.ItemUnavailable);
                case BillingResponseCode.Ok:
                    return null;
                case BillingResponseCode.ServiceDisconnected:
                    return new InAppBillingPurchaseException(PurchaseError.ServiceDisconnected);
                case BillingResponseCode.ServiceTimeout:
                    return new InAppBillingPurchaseException(PurchaseError.ServiceTimeout);
                case BillingResponseCode.ServiceUnavailable:
                    return new InAppBillingPurchaseException(PurchaseError.ServiceUnavailable);
                case BillingResponseCode.UserCancelled:
                    return null;
                default:
                    return null;

            }

        }

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// IOS only, not implemented for Android
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
    }
}
