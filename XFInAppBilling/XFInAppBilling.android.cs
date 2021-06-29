using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.BillingClient.Api;
using Android.Content;


namespace Plugin.XFInAppBilling
{
    /// <summary>
    /// Android Implementation
    /// </summary>
    [Preserve(AllMembers = true)]
    public class XFInAppBillingImplementation : Java.Lang.Object, IXFInAppBilling, IBillingClientStateListener, IPurchasesUpdatedListener, IAcknowledgePurchaseResponseListener, IPurchasesResponseListener
    {
        private bool _isServiceConnected;

        Activity Activity =>
            Xamarin.Essentials.Platform.CurrentActivity ?? throw new NullReferenceException("Current Activity is null, ensure that the MainActivity.cs file is configuring Xamarin.Essentials in your source code so the In App Billing can use it.");

        private Context CurrentContext => Application.Context;

        /// <summary>
        /// BillingClient to call api functions
        /// </summary>
        public BillingClient? BillingClient { get; set; }

        // private PurchaseResult PurchaseResult { get; set; } = new PurchaseResult();
        private List<PurchaseResult> PurchaseHistoryResult { get; set; } = new List<PurchaseResult>();

        private List<InAppBillingProduct> InAppBillingProducts { get; set; } = new List<InAppBillingProduct>();

        TaskCompletionSource<bool> _tcsConnect;
        TaskCompletionSource<PurchaseResult> _tcsPurchase;
        TaskCompletionSource<List<PurchaseResult>> _tcsPurchases;
        TaskCompletionSource<bool> _tcsAcknowledge;


        #region API Functions

        /// <summary>
        /// Start a connection on billingclient
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            _tcsConnect = new TaskCompletionSource<bool>();

            BillingClient = BillingClient.NewBuilder(CurrentContext).EnablePendingPurchases().SetListener(this).Build();

            BillingClient.StartConnection(this);

            return await _tcsConnect.Task;
        }

        /// <summary>
        /// Get Product Information with Prices
        /// </summary>
        /// <param name="productIds">Skus of products</param>
        /// <param name="itemType">Subscription or iap product</param>
        /// <returns></returns>
        public async Task<List<InAppBillingProduct>> GetProductsAsync(List<string> productIds, ItemType itemType = ItemType.InAppPurchase)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            var prms = SkuDetailsParams.NewBuilder();
            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            prms.SetSkusList(productIds).SetType(type);

            var result = await BillingClient?.QuerySkuDetailsAsync(prms.Build());

            return OnSkuDetailsResponse(result);
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

            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            var response = await BillingClient?.QueryPurchaseHistoryAsync(type);
            if (response != null)
                return OnPurchaseHistoryResponse(response.Result, response.PurchaseHistoryRecords);
            else
                return new List<PurchaseResult>();
        }

        /// <summary>
        /// Get All purchases regardless of status
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="verifyPurchase"></param>
        /// <returns></returns>
        public async Task<List<PurchaseResult>> GetPurchasesAsync(ItemType itemType, IInAppBillingVerifyPurchase? verifyPurchase = null, string? verifyOnlyProductId = null)
        {
            List<PurchaseResult> purchases = new List<PurchaseResult>();
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            var prms = SkuDetailsParams.NewBuilder();
            var type = itemType == ItemType.InAppPurchase ? BillingClient.SkuType.Inapp : BillingClient.SkuType.Subs;
            BillingClient?.QueryPurchasesAsync(type, this);
            return await _tcsPurchases.Task;
        }

        /// <summary>
        /// temporarily holds the product to purchase
        /// </summary>
        private SkuDetails? ProductToPurcase { get; set; }

        /// <summary>
        /// Does a purchase on BillingClient
        /// </summary>
        /// <param name="productId">Sku of Product or Subscription to purchase</param>
        /// <param name="itemType">subscription or iap product</param>
        /// <param name="payload">developer payload to verify</param>
        /// <param name="verifyPurchase">not used, only for IOS</param>
        /// <returns></returns>
        public async Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType = ItemType.InAppPurchase, string payload = null, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            PurchaseResult purchaseResult;
            var productIds = new List<string> { productId };

            await GetProductsAsync(productIds, itemType);

            if (ProductToPurcase != null)
            {
                purchaseResult = await DoPurchaseAsync(ProductToPurcase);
            }
            else
            {
                throw new Exception("Purchase Product not found");
            }

            return purchaseResult;

        }

        /// <summary>
        /// Consumes a consumable product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="purchaseToken"></param>
        /// <returns></returns>
        public async Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            return await CompleteConsume(purchaseToken);
        }

        /// <summary>
        /// Consumes a given in-app product. Consuming can only be done on an item that's owned,
        /// and as a result of consumption, the user will no longer own it.
        /// </summary>
        /// <param name="productId">Sku of the consumable product</param>
        /// <param name="itemType">product</param>
        /// <param name="payload">Deprecated for Android after 2.2 version</param>
        /// <param name="verifyPurchase"></param>
        /// <returns></returns>
        public async Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase? verifyPurchase = null)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }

            var purchases = await GetPurchasesAsync(itemType, verifyPurchase);

            var purchase = purchases?.FirstOrDefault(p => p.Sku == productId && p.DeveloperPayload == payload && p.ConsumptionState == ConsumptionState.NoYetConsumed);

            if (purchase is null)
            {
                purchase = purchases?.FirstOrDefault(p => p.Sku == productId && p.DeveloperPayload == payload);
            }

            if (purchase is null)
            {
                Console.WriteLine("Unable to find a purchase with matching product id and payload");
                throw new Exception("Unable to find a purchase with matching product id and payload");
            }

            return await CompleteConsume(purchase.PurchaseToken) ?? new PurchaseResult { PurchaseState = PurchaseState.Failed };
        }

        /// <summary>
        /// Disconnects from BillingClient
        /// </summary>
        /// <returns></returns>
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
        /// <param name="purchase"></param>
        /// <returns></returns>
        private async Task<bool> NotifyFullFillmentAsync(Purchase purchase)
        {
            if (purchase == null)
                return false;

            if (!purchase.IsAcknowledged)
            {
                if (BillingClient == null || !BillingClient.IsReady)
                {
                    await ConnectAsync();
                }

                _tcsAcknowledge = new TaskCompletionSource<bool>();

                AcknowledgePurchaseParams acknowledgePurchaseParams =
                                        AcknowledgePurchaseParams.NewBuilder()
                                            .SetPurchaseToken(purchase.PurchaseToken)
                                            .Build();
                BillingClient?.AcknowledgePurchase(acknowledgePurchaseParams, this);

                return await _tcsAcknowledge.Task;
            }
            return true;
        }

        /// <summary>
        /// Completes Consume purchase with Api request
        /// </summary>
        /// <param name="purchaseToken"></param>
        /// <returns></returns>
        private async Task<PurchaseResult> CompleteConsume(string purchaseToken)
        {

            var consumeParams = ConsumeParams.NewBuilder().SetPurchaseToken(purchaseToken);

            var response = await BillingClient?.ConsumeAsync(consumeParams.Build());
            if (response is null)
                throw new Exception("An error occured");

            return await OnConsumeResponse(response.BillingResult, response.PurchaseToken) ?? new PurchaseResult { PurchaseState = PurchaseState.Failed };
        }

        #endregion

        /// <summary>
        /// Completes the Purchase
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        private async Task<PurchaseResult> DoPurchaseAsync(SkuDetails product)
        {
            if (BillingClient == null || !BillingClient.IsReady)
            {
                await ConnectAsync();
            }
            _tcsPurchase = new TaskCompletionSource<PurchaseResult>();

            BillingFlowParams flowParams = BillingFlowParams.NewBuilder().SetSkuDetails(product).Build();
            BillingClient?.LaunchBillingFlow(Activity, flowParams);
            return await _tcsPurchase.Task;
        }

        #region ResponseHandlers

        /// <summary>
        /// Purchase History Response Handler
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="purchases"></param>
        public List<PurchaseResult> OnPurchaseHistoryResponse(BillingResult billingResult, IList<PurchaseHistoryRecord> purchases)
        {

            PurchaseHistoryResult = new List<PurchaseResult>();
            if (billingResult.ResponseCode == BillingResponseCode.Ok || billingResult.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
            {
                if (purchases?.Count > 0)
                {
                    foreach (var purchase in purchases)
                    {
                        var purchaseHistory = new PurchaseResult
                        {
                            Sku = purchase.Skus[0],
                            Skus = purchase.Skus,
                            PurchaseToken = purchase.PurchaseToken
                        };

                        if (purchase.PurchaseTime > 0)
                            purchaseHistory.PurchaseDate = DateTimeOffset.FromUnixTimeMilliseconds(purchase.PurchaseTime).DateTime;

                        purchaseHistory.DeveloperPayload = purchase.DeveloperPayload;

                        PurchaseHistoryResult.Add(purchaseHistory);
                    }
                }

                return PurchaseHistoryResult;
            }
            else
            {
                var errorCode = GetErrorCode(billingResult);
                if (errorCode != null)
                    throw errorCode;

                return PurchaseHistoryResult;
            }
        }

        /// <summary>
        /// Purchase Acknowledge Handler
        /// </summary>
        /// <param name="billingResult">returns BillingResult</param>
        public void OnAcknowledgePurchaseResponse(BillingResult billingResult)
        {
            CheckResultNotNull(billingResult);

            try
            {
                var isAcknowledged = billingResult.ResponseCode == BillingResponseCode.Ok;

                var errorCode = GetErrorCode(billingResult);
                if (errorCode != null) //No error
                {
                    _tcsAcknowledge?.TrySetException(errorCode);
                }
                else
                {
                    _tcsAcknowledge?.TrySetResult(isAcknowledged);
                }
            }
            catch (Exception ex)
            {
                _tcsAcknowledge?.TrySetException(ex);
            }
        }

        /// <summary>
        /// Purchase Handler - PurchasesUpdatedListener
        /// Listener interface for purchase updates which happen when, 
        /// for example, the user buys something within the app or by initiating a purchase from Google Play Store.
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="purchases"></param>
        public async void OnPurchasesUpdated(BillingResult billingResult, IList<Purchase>? purchases)
        {
            CheckResultNotNull(billingResult);

            var purchaseResult = await GetPurchaseResult(billingResult, purchases);

            var errorCode = GetErrorCode(billingResult);
            if (errorCode != null) //No error
            {
                _tcsPurchase?.TrySetException(errorCode);
            }
            else
            {
                _tcsPurchase?.TrySetResult(purchaseResult ?? new PurchaseResult() { PurchaseState = PurchaseState.Failed });
            }
        }

        /// <summary>
        /// Returns the purchase result after a purchase or consume
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="purchases"></param>
        /// <returns></returns>
        private async Task<PurchaseResult?> GetPurchaseResult(BillingResult billingResult, IList<Purchase>? purchases)
        {
            var purchaseResult = new PurchaseResult();

            CheckResultNotNull(billingResult);

            if (billingResult.ResponseCode == BillingResponseCode.Ok && purchases != null)
            {
                purchaseResult.PurchaseState = PurchaseState.Purchased;
                if (purchases?.Count > 0)
                {
                    var purchaseResults = await GetPurchasesAsync(purchases);
                    purchaseResult = purchaseResults?.OrderByDescending(p => p.ExpirationDate).Last();
                }
            }
            else if (billingResult.ResponseCode == BillingResponseCode.UserCancelled)
            {
                purchaseResult.PurchaseState = PurchaseState.Cancelled;
            }
            else if (billingResult.ResponseCode == BillingResponseCode.ItemAlreadyOwned)
            {
                purchaseResult.PurchaseState = PurchaseState.AlreadyOwned;
            }
            else
            {
                purchaseResult.PurchaseState = PurchaseState.Failed;
            }

            return purchaseResult;
        }

        private static void CheckResultNotNull(BillingResult billingResult)
        {
            if (billingResult == null)
            {
                throw new InAppBillingPurchaseException(PurchaseError.GeneralError, "BillingResult null returned");
            }
        }

        /// <summary>
        /// GetPurchases handler
        /// </summary>
        /// <param name="purchases"></param>
        /// <returns></returns>
        private async Task<List<PurchaseResult>> GetPurchasesAsync(IList<Purchase> purchases)
        {
            var purchaseResults = new List<PurchaseResult>();
            if (purchases?.Count > 0)
            {
                foreach (var purchase in purchases)
                {
                    var purchaseResult = new PurchaseResult();
                    PurchaseState purchaseState;
                    switch (purchase.PurchaseState)
                    {
                        case Android.BillingClient.Api.PurchaseState.Pending:
                            purchaseState = PurchaseState.Pending;
                            break;
                        case Android.BillingClient.Api.PurchaseState.Purchased:
                            // Acknowledge the purchase if it hasn't already been acknowledged.
                            if (await NotifyFullFillmentAsync(purchase))
                                purchaseState = PurchaseState.Purchased;
                            else
                                purchaseState = PurchaseState.NotAknowledged;
                            break;
                        case Android.BillingClient.Api.PurchaseState.Unspecified:
                            purchaseState = PurchaseState.Unspecified;
                            break;
                        default:
                            purchaseState = PurchaseState.Unspecified;
                            break;
                    }
                    purchaseResult.Sku = purchase.Skus[0];
                    purchaseResult.Skus = purchase.Skus;
                    purchaseResult.Quantity = purchase.Quantity;
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

        /// <summary>
        /// Sku/Product details Handler
        /// </summary>
        /// <param name="querySkuDetailsResult"></param>
        /// <returns></returns>
        public List<InAppBillingProduct> OnSkuDetailsResponse(QuerySkuDetailsResult querySkuDetailsResult)
        {
            InAppBillingProducts = new List<InAppBillingProduct>();
            if (querySkuDetailsResult.Result.ResponseCode == BillingResponseCode.Ok)
            {

                // List<string> unavailableSkus = args.UnavailableSkus;
                if (querySkuDetailsResult.SkuDetails?.Count > 0)
                {
                    foreach (var product in querySkuDetailsResult.SkuDetails)
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
                            IntroductoryPrice = product.IntroductoryPrice,
                            IntroductoryPriceCycles = product.IntroductoryPriceCycles,
                            IntroductoryPricePeriod = product.IntroductoryPricePeriod,
                            SubscriptionPeriod = product.SubscriptionPeriod,
                            FreeTrialPeriod = product.FreeTrialPeriod,
                            OriginalPrice = product.OriginalPrice,
                            OriginalPriceAmountMicros = product.OriginalPriceAmountMicros
                        });
                    }

                    ProductToPurcase = querySkuDetailsResult.SkuDetails[0];
                }
            }

            var errorCode = GetErrorCode(querySkuDetailsResult.Result);
            if (errorCode != null) //No error
            {
                throw errorCode;
            }
            else
            {
                return InAppBillingProducts;
            }
        }

        /// <summary>
        /// Disconnect Handler
        /// </summary>
        public async void OnBillingServiceDisconnected()
        {
            try
            {
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _tcsConnect?.TrySetException(ex);
            }
        }

        /// <summary>
        /// Connect Handler
        /// </summary>
        /// <param name="billingResult"></param>
        public void OnBillingSetupFinished(BillingResult billingResult)
        {
            CheckResultNotNull(billingResult);

            try
            {
                if (billingResult.ResponseCode == BillingResponseCode.Ok)
                {
                    _isServiceConnected = true;
                    _tcsConnect?.TrySetResult(_isServiceConnected);
                }
                else
                {
                    var exception = GetErrorCode(billingResult);
                    _tcsConnect?.TrySetException(exception);
                }
            }
            catch (Exception ex)
            {
                _tcsConnect?.TrySetException(ex);
            }
        }

        /// <summary>
        /// The listener for the result of the Consume returned asynchronously through the callback with the BillingResult and purchase token.
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="purchaseToken"></param>
        /// <returns></returns>
        public async Task<PurchaseResult?> OnConsumeResponse(BillingResult billingResult, string purchaseToken)
        {
            CheckResultNotNull(billingResult);

            var purchaseResult = await GetPurchaseResult(billingResult, null);

            var errorCode = GetErrorCode(billingResult);
            if (errorCode != null) //No error
            {
                throw errorCode;
            }
            else
            {
                return purchaseResult;
            }
        }

        /// <summary>
        /// The listener for the result of the query returned asynchronously through the callback with the BillingResult and the list of Purchase.
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="purchases"></param>
        public async void OnQueryPurchasesResponse(BillingResult billingResult, IList<Purchase> purchases)
        {
            var errorCode = GetErrorCode(billingResult);
            if (errorCode != null) //No error
            {
                throw errorCode;
            }
            else
            {
                _tcsPurchases = new TaskCompletionSource<List<PurchaseResult>>();
                var result = await GetPurchasesAsync(purchases);
                _tcsPurchases.SetResult(result);
            }
        }

        #endregion

        /// <summary>
        /// Dispose and disconnect from BillingClient
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disconnect();
        }

        /// <summary>
        /// Returns Exception if return code is not OK
        /// </summary>
        /// <param name="billingResponseCode"></param>
        /// <returns></returns>
        private InAppBillingPurchaseException? GetErrorCode(BillingResult billingResult)
        {
            BillingResponseCode billingResponseCode = billingResult.ResponseCode;
            string message = billingResult.DebugMessage ?? "";

            return billingResponseCode switch
            {
                BillingResponseCode.Ok => null,
                BillingResponseCode.BillingUnavailable => new InAppBillingPurchaseException(PurchaseError.BillingUnavailable, message),
                BillingResponseCode.DeveloperError => new InAppBillingPurchaseException(PurchaseError.DeveloperError, message),
                BillingResponseCode.Error => new InAppBillingPurchaseException(PurchaseError.GeneralError, message),
                BillingResponseCode.FeatureNotSupported => new InAppBillingPurchaseException(PurchaseError.FeatureNotSupported, message),
                BillingResponseCode.ItemAlreadyOwned => null,
                BillingResponseCode.ItemNotOwned => new InAppBillingPurchaseException(PurchaseError.NotOwned, message),
                BillingResponseCode.ItemUnavailable => new InAppBillingPurchaseException(PurchaseError.ItemUnavailable, message),
                BillingResponseCode.ServiceDisconnected => new InAppBillingPurchaseException(PurchaseError.ServiceDisconnected, message),
                BillingResponseCode.ServiceTimeout => new InAppBillingPurchaseException(PurchaseError.ServiceTimeout, message),
                BillingResponseCode.ServiceUnavailable => new InAppBillingPurchaseException(PurchaseError.ServiceUnavailable, message),
                BillingResponseCode.UserCancelled => new InAppBillingPurchaseException(PurchaseError.UserCancelled, message),
                _ => new InAppBillingPurchaseException(PurchaseError.GeneralError, message),
            };
        }

        #region NOTUSED FOR ANDROID
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


        #endregion
    }
}
