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
        public string Sku { get; set; }    
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

        /// <summary>
        /// Gets the current consumption state
        /// </summary>
        public ConsumptionState ConsumptionState { get; set; }
 
        public static bool operator ==(PurchaseResult left, PurchaseResult right) =>
            Equals(left, right);

        public static bool operator !=(PurchaseResult left, PurchaseResult right) =>
            !Equals(left, right);

        public override bool Equals(object obj) =>
            (obj is PurchaseResult purchase) && Equals(purchase);

        public bool Equals(PurchaseResult other) =>
            (OrderId, Sku, IsAutoRenewing, PurchaseToken, PurchaseState, DeveloperPayload) ==
            (other.OrderId, other.Sku, other.IsAutoRenewing, other.PurchaseToken, other.PurchaseState, other.DeveloperPayload);

        public override int GetHashCode() =>
            (OrderId, Sku, IsAutoRenewing, PurchaseToken, PurchaseState, DeveloperPayload).GetHashCode();

        /// <summary>
        /// Prints out product
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"Sku:{Sku} | IsAutoRenewing:{IsAutoRenewing} | PurchaseState:{PurchaseState} | OrderId:{OrderId}"; 
       
    }
}
