﻿using System;

namespace Plugin.XFInAppBilling
{
    [Preserve(AllMembers = true)]
    /// <summary>
    /// Type of purchase error
    /// </summary>
    public enum PurchaseError
    {
        /// <summary>
        /// Billing API version is not supported for the type requested (Android), client error (iOS)
        /// </summary>
        BillingUnavailable,
        /// <summary>
        /// Developer issue. Invalid arguments provided to the API.
        /// </summary>
        DeveloperError,
        /// <summary>
        /// Product sku not available
        /// </summary>
        ItemUnavailable,
        /// <summary>
        /// Android:Fatal error during the API action
        /// </summary>
        GeneralError,
        /// <summary>
        /// User cancelled the purchase
        /// </summary>
        UserCancelled,
        /// <summary>
        /// App store unavailable on device
        /// </summary>
        AppStoreUnavailable,
        /// <summary>
        /// User is not allowed to authorize payments
        /// </summary>
        PaymentNotAllowed,
        /// <summary>
        /// One of the payment parameters was not recognized by app store
        /// </summary>
        PaymentInvalid,
        /// <summary>
        /// The requested product is invalid
        /// </summary>
        InvalidProduct,
        /// <summary>
        /// The product request failed
        /// </summary>
        ProductRequestFailed,
        /// <summary>
        /// Restoring the transaction failed
        /// </summary>
        RestoreFailed,
        /// <summary>
        /// Network connection is down
        /// </summary>
        ServiceUnavailable,
        /// <summary>
        /// Product is already owned
        /// </summary>
        AlreadyOwned,
        /// <summary>
        /// Item is not owned and can not be consumed
        /// </summary>
        NotOwned,
        /// <summary>
        /// Android:Requested feature is not supported by Play Store on the current device.
        /// </summary>
        FeatureNotSupported,
        /// <summary>
        /// Play Store service is not connected now - potentially transient state.
        /// </summary>
        ServiceDisconnected,
        /// <summary>
        /// The request has reached the maximum timeout before Google Play responds.
        /// </summary>
        ServiceTimeout,
        AppleTermsConditionsChanged
    }

   
    /// <summary>
    /// Purchase exception
    /// </summary>
    public class InAppBillingPurchaseException : Exception
    {
        /// <summary>
        /// Type of error
        /// </summary>
        public PurchaseError PurchaseError { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="ex"></param>
        public InAppBillingPurchaseException(PurchaseError error, Exception ex, string message) : base(message, ex)
        {
            PurchaseError = error;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public InAppBillingPurchaseException(PurchaseError error, string message) : base(message)
        {
            PurchaseError = error;
        }
    }
}