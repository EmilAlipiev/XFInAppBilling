using System;
using System.Collections.Generic;

namespace Plugin.XFInAppBilling
{
    [Preserve(AllMembers = true)]
    public class InAppBillingPurchaseComparer : IEqualityComparer<PurchaseResult>
    {
        public bool Equals(PurchaseResult x, PurchaseResult y) => x.Equals(y);

        public int GetHashCode(PurchaseResult x) => x.GetHashCode();
    }

    [Preserve(AllMembers = true)]
    public class PurchaseResult : IEquatable<PurchaseResult>
    {
        public PurchaseResult()
        {
                
        }
        /// <summary>
        /// ProductId of Purchased Item
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Products of Purchased Items. if multiple skus are purchased with 1 billing request. For subscriptions always 1
        /// </summary>
        public IList<string> Products { get; set; }
        public string PurchaseToken { get; set; }
        public string OrderId { get; set; }
        public bool IsAutoRenewing { get; set; }
        public PurchaseState PurchaseState { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public bool IsAcknowledged { get; set; }
        public string DeveloperPayload { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public string UserId { get; set; }
        public string ItemType { get; set; }

        public string ObfuscatedAccountId { get; set; }

        public string ObfuscatedProfileId { get; set; }

        /// <summary>
        /// Gets the current consumption state
        /// </summary>
        public ConsumptionState ConsumptionState { get; set; }
        /// <summary>
        /// Returns the quantity of the purchased product
        /// Android:Always returns 1 for BillingClient.SkuType.SUBS items; could be greater than 1 for BillingClient.SkuType.INAPP items.
        /// </summary>
        public int Quantity { get;  set; }
        /// <summary>
        /// Indicates whether the subscritpion renewes automatically. If true, the sub is active, else false the user has canceled.
        /// </summary>
        public bool AutoRenewing { get; set; }

        public string OriginalJson { get; set; }
        public string Signature { get; set; }

        public static bool operator ==(PurchaseResult left, PurchaseResult right) =>
            Equals(left, right);

        public static bool operator !=(PurchaseResult left, PurchaseResult right) =>
            !Equals(left, right);

        public override bool Equals(object obj) =>
            (obj is PurchaseResult purchase) && Equals(purchase);
        public bool Equals(PurchaseResult other) =>
                (OrderId, PurchaseDate, IsAcknowledged, ProductId, AutoRenewing, PurchaseToken, PurchaseState, DeveloperPayload, ObfuscatedAccountId, ObfuscatedProfileId, Quantity, Products, OriginalJson, Signature) ==
                (other.OrderId, other.PurchaseDate, other.IsAcknowledged, other.ProductId, other.AutoRenewing, other.PurchaseToken, other.PurchaseState, other.DeveloperPayload, other.ObfuscatedAccountId, other.ObfuscatedProfileId, other.Quantity, other.Products, other.OriginalJson, other.Signature);

        public override int GetHashCode() =>
            (OrderId, ProductId, IsAutoRenewing, PurchaseToken, PurchaseState, DeveloperPayload).GetHashCode();

        /// <summary>
        /// Prints out product
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"{nameof(ProductId)}:{ProductId}| {nameof(IsAcknowledged)}:{IsAcknowledged} | {nameof(AutoRenewing)}:{AutoRenewing} | {nameof(PurchaseState)}:{PurchaseState} | {nameof(OrderId)}:{OrderId} | {nameof(ObfuscatedAccountId)}:{ObfuscatedAccountId}  | {nameof(ObfuscatedProfileId)}:{ObfuscatedProfileId}  | {nameof(Signature)}:{Signature}  | {nameof(OriginalJson)}:{OriginalJson}  | {nameof(Quantity)}:{Quantity}";


    }
}
