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



 ### AMAZON IAP

Amazon Iap implementation isn't included into the Nuget package. Because Amazon app is also an android project and using plugin, it is currently not possible to implement 2 android project, especially multi-targeting doesn't seem to allow it (if anyone knows how to do it, please feel free to implement and send PR)

I could include AmazonIAP dll and implementation into the nuget package but why should I? it is messy you will have Google billingclient and amazon iap dll unnecessarily included into same project. 

In general you should create another android project for Amazon (see the test project in the source code) and separate from google Android project, if you have shared resources, code and other things, create and Android Library project and reference on Main Google Android and Amazon Android projects. So you will have a shared Android Library project. Thus you can keep Google specific implementation and Amazon specific implementation clean and separate.

If you follow above suggestion, 
-  install the XFInAppBilling Nuget on .net standard, android, uwp and ios projects (dont install on amazon project)
-  Copy  [this folder]
into your amazon android project
- reference the AmazonIapV2Android.dll using add reference
- Adjust the namespace inside this class InAppBillingImplementation.cs 
You should be good to go :)

Here is how you can work with it

 [this folder]: <https://github.com/EmilAlipiev/XFInAppBilling/tree/Master/XFInAppBilling.Tests/XFInAppBilling.Tests.Amazon/IAP>
