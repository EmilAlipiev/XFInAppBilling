using Foundation;
using StoreKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.XFInAppBilling
{     
    public class XFInAppBillingImplementation : IXFInAppBilling
    {
        static bool IsiOS112 => UIDevice.CurrentDevice.CheckSystemVersion(11, 2);

        /// <summary>
        /// Default constructor for In App Billing on iOS
        /// </summary>
        public InAppBilling()
        {
            paymentObserver = new PaymentObserver(OnPurchaseComplete, OnShouldAddStorePayment);
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(paymentObserver);
        }

        #region API Functions
        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(true);
        }

        public bool Disconnect()
        {
            return true;
        }

        /// <summary>
        /// Get product information of a specific product
        /// </summary>
        /// <param name="productIds">Sku or Id of the product(s)</param>
        /// <param name="itemType">Type of product offering</param>
        /// <returns></returns>
        public async Task<List<InAppBillingProduct>> GetProductsAsync(List<string> productIds, ItemType itemType = ItemType.InAppPurchase)
        {
            var products = await GetProductAsync(productIds);

            return products.Select(p => new InAppBillingProduct
            {
                LocalizedPrice = p.LocalizedPrice(),
                MicrosPrice = (long)(p.Price.DoubleValue * 1000000d),
                Name = p.LocalizedTitle,
                ProductId = p.ProductIdentifier,
                Description = p.LocalizedDescription,
                CurrencyCode = p.PriceLocale?.CurrencyCode ?? string.Empty,
                LocalizedIntroductoryPrice = IsiOS112 ? (p.IntroductoryPrice?.LocalizedPrice() ?? string.Empty) : string.Empty,
                MicrosIntroductoryPrice = IsiOS112 ? (long)((p.IntroductoryPrice?.Price?.DoubleValue ?? 0) * 1000000d) : 0
            }).ToList();
        }

        public async Task<IEnumerable<PurchaseResult>> GetPurchasesAsync(ItemType itemType, Plugin.InAppBilling.Abstractions.IInAppBillingVerifyPurchase verifyPurchase, string verifyOnlyProductId)
        {
            var purchases = await RestoreAsync();

            if (purchases == null)
                return null;

            var comparer = new PurchaseResultComparer();
            var converted = purchases
                .Where(p => p != null)
                .Select(p2 => p2.ToIABPurchase())
                .Distinct(comparer);

            var validPurchases = new List<PurchaseResult>();
            foreach (var purchase in converted)
            {
                if ((verifyOnlyProductId != null && !verifyOnlyProductId.Equals(purchase.Sku)) || await ValidateReceipt(verifyPurchase, purchase.Sku, purchase.OrderId))
                    validPurchases.Add(purchase);
            }

            return validPurchases.Any() ? validPurchases : null;
        }

        /// <summary>
        /// Purchase a specific product or subscription
        /// </summary>
        /// <param name="productId">Sku or ID of product</param>
        /// <param name="itemType">Type of product being requested</param>
        /// <param name="payload">Developer specific payload</param>
        /// <param name="verifyPurchase">Interface to verify purchase</param>
        /// <returns></returns>
        public async Task<PurchaseResult> PurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null)
        {
            var p = await PurchaseAsync(productId);

            var reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            var purchase = new PurchaseResult
            {
                PurchaseDate = reference.AddSeconds(p.TransactionDate.SecondsSinceReferenceDate),
                OrderId = p.TransactionIdentifier,
                Sku = p.Payment?.ProductIdentifier ?? string.Empty,
                PurchaseState = p.GetPurchaseState(),
                PurchaseToken = p.TransactionReceipt?.GetBase64EncodedString(NSDataBase64EncodingOptions.None) ?? string.Empty
            };

            if (verifyPurchase == null)
                return purchase;

            var validated = await ValidateReceipt(verifyPurchase, purchase.Sku, purchase.OrderId);

            return validated ? purchase : null;
        }


        public Task<bool> CheckIfUserHasActiveSubscriptionAsync(string subscriptionId, ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }


        public Task<List<PurchaseResult>> GetPurchaseHistory(ItemType itemType = ItemType.InAppPurchase)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Gets or sets a callback for out of band purchases to complete.
        /// </summary>
        public static Action<PurchaseResult> OnPurchaseComplete { get; set; } = null;

        public static Func<SKPaymentQueue, SKPayment, SKProduct, bool> OnShouldAddStorePayment { get; set; } = null;

        private Task<IEnumerable<SKProduct>> GetProductAsync(List<string> productId)
        {
            var productIdentifiers = NSSet.MakeNSObjectSet<NSString>(productId.Select(i => new NSString(i)).ToArray());

            var productRequestDelegate = new ProductRequestDelegate();

            //set up product request for in-app purchase
            var productsRequest = new SKProductsRequest(productIdentifiers)
            {
                Delegate = productRequestDelegate // SKProductsRequestDelegate.ReceivedResponse
            };
            productsRequest.Start();

            return productRequestDelegate.WaitForResponse();
        }

        Task<SKPaymentTransaction[]> RestoreAsync()
        {
            var tcsTransaction = new TaskCompletionSource<SKPaymentTransaction[]>();

            var allTransactions = new List<SKPaymentTransaction>();

            Action<SKPaymentTransaction[]> handler = null;
            handler = new Action<SKPaymentTransaction[]>(transactions =>
            {

                // Unsubscribe from future events
                paymentObserver.TransactionsRestored -= handler;

                if (transactions == null)
                {
                    if (allTransactions.Count == 0)
                        tcsTransaction.TrySetException(new PurchaseResultException(PurchaseError.RestoreFailed, "Restore Transactions Failed"));
                    else
                        tcsTransaction.TrySetResult(allTransactions.ToArray());
                }
                else
                {
                    allTransactions.AddRange(transactions);
                    tcsTransaction.TrySetResult(allTransactions.ToArray());
                }
            });

            paymentObserver.TransactionsRestored += handler;

            foreach (var trans in SKPaymentQueue.DefaultQueue.Transactions)
            {
                var original = FindOriginalTransaction(trans);
                if (original == null)
                    continue;

                allTransactions.Add(original);
            }

            // Start receiving restored transactions
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();

            return tcsTransaction.Task;
        }

        static SKPaymentTransaction FindOriginalTransaction(SKPaymentTransaction transaction)
        {
            if (transaction == null)
                return null;

            if (transaction.TransactionState == SKPaymentTransactionState.Purchased ||
                transaction.TransactionState == SKPaymentTransactionState.Purchasing)
                return transaction;

            if (transaction.OriginalTransaction != null)
                return FindOriginalTransaction(transaction.OriginalTransaction);

            return transaction;

        }

        Task<bool> ValidateReceipt(IInAppBillingVerifyPurchase verifyPurchase, string productId, string transactionId)
        {
            if (verifyPurchase == null)
                return Task.FromResult(true);

            // Get the receipt data for (server-side) validation.
            // See: https://developer.apple.com/library/content/releasenotes/General/ValidateAppStoreReceipt/Introduction.html#//apple_ref/doc/uid/TP40010573
            NSData receiptUrl = null;
            if (NSBundle.MainBundle.AppStoreReceiptUrl != null)
                receiptUrl = NSData.FromUrl(NSBundle.MainBundle.AppStoreReceiptUrl);

            var receipt = receiptUrl?.GetBase64EncodedString(NSDataBase64EncodingOptions.None);

            return verifyPurchase.VerifyPurchase(receipt, string.Empty, productId, transactionId);
        }


        Task<SKPaymentTransaction> PurchaseAsync(string productId)
        {
            var tcsTransaction = new TaskCompletionSource<SKPaymentTransaction>();

            Action<SKPaymentTransaction, bool> handler = null;
            handler = new Action<SKPaymentTransaction, bool>((tran, success) =>
            {
                if (tran?.Payment == null)
                    return;

                // Only handle results from this request
                if (productId != tran.Payment.ProductIdentifier)
                    return;

                // Unsubscribe from future events
                paymentObserver.TransactionCompleted -= handler;

                if (success)
                {
                    tcsTransaction.TrySetResult(tran);
                    return;
                }

                var errorCode = tran?.Error?.Code ?? -1;
                var description = tran?.Error?.LocalizedDescription ?? string.Empty;
                var error = PurchaseError.GeneralError;
                switch (errorCode)
                {
                    case (int)SKError.PaymentCancelled:
                        error = PurchaseError.UserCancelled;
                        break;
                    case (int)SKError.PaymentInvalid:
                        error = PurchaseError.PaymentInvalid;
                        break;
                    case (int)SKError.PaymentNotAllowed:
                        error = PurchaseError.PaymentNotAllowed;
                        break;
                    case (int)SKError.ProductNotAvailable:
                        error = PurchaseError.ItemUnavailable;
                        break;
                    case (int)SKError.Unknown:
                        error = PurchaseError.GeneralError;
                        break;
                    case (int)SKError.ClientInvalid:
                        error = PurchaseError.BillingUnavailable;
                        break;
                }

                tcsTransaction.TrySetException(new InAppBillingPurchaseException(error, description));

            });

            paymentObserver.TransactionCompleted += handler;

            var payment = SKPayment.CreateFrom(productId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);

            return tcsTransaction.Task;
        }


        /// <summary>
        /// Consume a purchase with a purchase token.
        /// </summary>
        /// <param name="productId">Id or Sku of product</param>
        /// <param name="purchaseToken">Original Purchase Token</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        public override Task<PurchaseResult> ConsumePurchaseAsync(string productId, string purchaseToken) =>
            null;

        /// <summary>
        /// Consume a purchase
        /// </summary>
        /// <param name="productId">Id/Sku of the product</param>
        /// <param name="payload">Developer specific payload of original purchase</param>
        /// <param name="itemType">Type of product being consumed.</param>
        /// <param name="verifyPurchase">Verify Purchase implementation</param>
        /// <returns>If consumed successful</returns>
        /// <exception cref="InAppBillingPurchaseException">If an error occures during processing</exception>
        public override Task<PurchaseResult> ConsumePurchaseAsync(string productId, ItemType itemType, string payload, IInAppBillingVerifyPurchase verifyPurchase = null) =>
            null;

        public override Task<bool> FinishTransaction(PurchaseResult purchase) =>
            FinishTransaction(purchase?.Id);

        public override async Task<bool> FinishTransaction(string purchaseId)
        {
            if (string.IsNullOrWhiteSpace(purchaseId))
                throw new ArgumentException("PurchaseId must be valid", nameof(purchaseId));

            var purchases = await RestoreAsync();

            if (purchases == null)
                return false;

            var transaction = purchases.Where(p => p.TransactionIdentifier == purchaseId).FirstOrDefault();
            if (transaction == null)
                return false;

            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);

            return true;
        }

        PaymentObserver paymentObserver;

        static DateTime NSDateToDateTimeUtc(NSDate date)
        {
            var reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);


            return reference.AddSeconds(date?.SecondsSinceReferenceDate ?? 0);
        }

        private bool disposed = false;


        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        public override void Dispose(bool disposing)
        {
            if (disposed)
            {
                base.Dispose(disposing);
                return;
            }

            disposed = true;

            if (!disposing)
            {
                base.Dispose(disposing);
                return;
            }

            if (paymentObserver != null)
            {
                SKPaymentQueue.DefaultQueue.RemoveTransactionObserver(paymentObserver);
                paymentObserver.Dispose();
                paymentObserver = null;
            }


            base.Dispose(disposing);
        }
    }


    [Preserve(AllMembers = true)]
    class ProductRequestDelegate : NSObject, ISKProductsRequestDelegate, ISKRequestDelegate
    {
        TaskCompletionSource<IEnumerable<SKProduct>> tcsResponse = new TaskCompletionSource<IEnumerable<SKProduct>>();

        public Task<IEnumerable<SKProduct>> WaitForResponse() =>
            tcsResponse.Task;


        [Export("request:didFailWithError:")]
        public void RequestFailed(SKRequest request, NSError error) =>
            tcsResponse.TrySetException(new InAppBillingPurchaseException(PurchaseError.ProductRequestFailed, error.LocalizedDescription));


        public void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
        {
            var product = response.Products;

            if (product != null)
            {
                tcsResponse.TrySetResult(product);
                return;
            }

            tcsResponse.TrySetException(new InAppBillingPurchaseException(PurchaseError.InvalidProduct, "Invalid Product"));
        }
    }

    [Preserve(AllMembers = true)]
    class PaymentObserver : SKPaymentTransactionObserver
    {
        public event Action<SKPaymentTransaction, bool> TransactionCompleted;
        public event Action<SKPaymentTransaction[]> TransactionsRestored;

        List<SKPaymentTransaction> restoredTransactions = new List<SKPaymentTransaction>();
        private readonly Action<PurchaseResult> onPurchaseSuccess;
        private readonly Func<SKPaymentQueue, SKPayment, SKProduct, bool> onShouldAddStorePayment;

        public PaymentObserver(Action<PurchaseResult> onPurchaseSuccess, Func<SKPaymentQueue, SKPayment, SKProduct, bool> onShouldAddStorePayment)
        {
            this.onPurchaseSuccess = onPurchaseSuccess;
            this.onShouldAddStorePayment = onShouldAddStorePayment;
        }

        public override bool ShouldAddStorePayment(SKPaymentQueue queue, SKPayment payment, SKProduct product)
        {
            return onShouldAddStorePayment?.Invoke(queue, payment, product) ?? false;
        }

        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            var rt = transactions.Where(pt => pt.TransactionState == SKPaymentTransactionState.Restored);

            // Add our restored transactions to the list
            // We might still get more from the initial request so we won't raise the event until
            // RestoreCompletedTransactionsFinished is called
            if (rt?.Any() ?? false)
                restoredTransactions.AddRange(rt);

            foreach (var transaction in transactions)
            {
                if (transaction?.TransactionState == null)
                    break;

                Debug.WriteLine($"Updated Transaction | {transaction.ToStatusString()}");

                switch (transaction.TransactionState)
                {
                    case SKPaymentTransactionState.Restored:
                    case SKPaymentTransactionState.Purchased:
                        TransactionCompleted?.Invoke(transaction, true);

                        onPurchaseSuccess?.Invoke(transaction.ToIABPurchase());

                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Failed:
                        TransactionCompleted?.Invoke(transaction, false);
                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            if (restoredTransactions == null)
                return;

            // This is called after all restored transactions have hit UpdatedTransactions
            // at this point we are done with the restore request so let's fire up the event
            var allTransactions = restoredTransactions.ToArray();

            // Clear out the list of incoming restore transactions for future requests
            restoredTransactions.Clear();

            TransactionsRestored?.Invoke(allTransactions);

            foreach (var transaction in allTransactions)
                SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
        }

        // Failure, just fire with null
        public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error) =>
            TransactionsRestored?.Invoke(null);

    }

    [Preserve(AllMembers = true)]
    static class SKTransactionExtensions
    {
        public static string ToStatusString(this SKPaymentTransaction transaction) =>
            transaction?.ToIABPurchase()?.ToString() ?? string.Empty;


        public static PurchaseResult ToIABPurchase(this SKPaymentTransaction transaction)
        {
            var p = transaction?.OriginalTransaction ?? transaction;

            if (p == null)
                return null;


            return new PurchaseResult
            {
                PurchaseDate = NSDateToDateTimeUtc(transaction.TransactionDate),
                OrderId = p.TransactionIdentifier,
                Sku = p.Payment?.ProductIdentifier ?? string.Empty,
                PurchaseState = p.GetPurchaseState(),
                PurchaseToken = p.TransactionReceipt?.GetBase64EncodedString(NSDataBase64EncodingOptions.None) ?? string.Empty
            };
        }

        static DateTime NSDateToDateTimeUtc(NSDate date)
        {
            var reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return reference.AddSeconds(date?.SecondsSinceReferenceDate ?? 0);
        }

        public static PurchaseState GetPurchaseState(this SKPaymentTransaction transaction)
        {
            if (transaction?.TransactionState == null)
                return PurchaseState.Unspecified;

            switch (transaction.TransactionState)
            {
                case SKPaymentTransactionState.Restored:
                    return PurchaseState.Restored;
                case SKPaymentTransactionState.Purchasing:
                    return PurchaseState.Purchasing;
                case SKPaymentTransactionState.Purchased:
                    return PurchaseState.Purchased;
                case SKPaymentTransactionState.Failed:
                    return PurchaseState.Failed;
                case SKPaymentTransactionState.Deferred:
                    return PurchaseState.Pending;
            }

            return PurchaseState.Unspecified;
        }
    }

    [Preserve(AllMembers = true)]
    static class SKProductExtension
    {
        /// <remarks>
        /// Use Apple's sample code for formatting a SKProduct price
        /// https://developer.apple.com/library/ios/#DOCUMENTATION/StoreKit/Reference/SKProduct_Reference/Reference/Reference.html#//apple_ref/occ/instp/SKProduct/priceLocale
        /// Objective-C version:
        ///    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
        ///    [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
        ///    [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
        ///    [numberFormatter setLocale:product.priceLocale];
        ///    NSString *formattedString = [numberFormatter stringFromNumber:product.price];
        /// </remarks>
        public static string LocalizedPrice(this SKProduct product)
        {
            if (product?.PriceLocale == null)
                return string.Empty;

            var formatter = new NSNumberFormatter()
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
                Locale = product.PriceLocale
            };
            var formattedString = formatter.StringFromNumber(product.Price);
            Console.WriteLine(" ** formatter.StringFromNumber(" + product.Price + ") = " + formattedString + " for locale " + product.PriceLocale.LocaleIdentifier);
            return formattedString;
        }

        public static string LocalizedPrice(this SKProductDiscount product)
        {
            if (product?.PriceLocale == null)
                return string.Empty;

            var formatter = new NSNumberFormatter()
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
                Locale = product.PriceLocale
            };
            var formattedString = formatter.StringFromNumber(product.Price);
            Console.WriteLine(" ** formatter.StringFromNumber(" + product.Price + ") = " + formattedString + " for locale " + product.PriceLocale.LocaleIdentifier);
            return formattedString;
        }

    }

    [Preserve(AllMembers = true)]
    public class PurchaseResultComparer : IEqualityComparer<PurchaseResult>
    {
        public bool Equals(PurchaseResult x, PurchaseResult y) => x.Equals(y);

        public int GetHashCode(PurchaseResult x) => x.GetHashCode();
    }
}
