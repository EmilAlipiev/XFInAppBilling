# XFInAppBilling

Cross platform in app billing library for Android, IOS and UWP. 
it uses billingclient api of Android and StoreContext from UWP. See below, function references implemented.

 ### Functions overview 

|  Function | Android  | Amazon  | UWP  | IOS   |
|---|---|---|---|---|
| PurchaseAsync |  [launchBillingFlow](https://developer.android.com/google/play/billing/billing_library_overview#Enable) |  [Purchase](https://developer.amazon.com/docs/cross-platform-plugins/cpp-use-the-iap-plugin-for-xamarin.html#purchase)|[RequestPurchaseAsync](https://docs.microsoft.com/en-us/windows/uwp/monetize/enable-subscription-add-ons-for-your-app#steps-to-enable-a-subscription-add-on-for-your-app)||
 | GetProductsAsync  |[querySkuDetailsAsync](https://developer.android.com/google/play/billing/billing_library_overview#Query)   |[GetProductData](https://developer.amazon.com/docs/cross-platform-plugins/cpp-use-the-iap-plugin-for-xamarin.html#getproductdata)|   [GetAssociatedStoreProductsAsync](https://docs.microsoft.com/en-us/windows/uwp/monetize/get-product-info-for-apps-and-add-ons#get-info-for-add-ons-that-are-available-for-purchase-from-the-current-app)|   |
| GetPurchases /CheckIfUserHasActiveSubscriptionAsync  |   [queryPurchaseHistoryAsync](https://developer.android.com/google/play/billing/billing_library_overview#Query-recent) |   [GetPurchaseUpdates](https://developer.amazon.com/docs/cross-platform-plugins/cpp-use-the-iap-plugin-for-xamarin.html#getpurchaseupdates) |  [GetAppLicenseAsync](https://docs.microsoft.com/en-us/windows/uwp/monetize/get-license-info-for-apps-and-add-ons) |   |
|  na |    [acknowledgePurchase](https://developer.android.com/google/play/billing/billing_library_overview#acknowledge)|  [NotifyFulfillment](https://developer.amazon.com/docs/cross-platform-plugins/cpp-use-the-iap-plugin-for-xamarin.html#notifyfulfillment)  |  na |   |
|  na |    |  [GetUserData](https://developer.amazon.com/docs/cross-platform-plugins/cpp-use-the-iap-plugin-for-xamarin.html#getuserdata)  |   |   |

 
