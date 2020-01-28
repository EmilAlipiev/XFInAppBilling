using System;

namespace Plugin.XFInAppBilling
{
    [Preserve(AllMembers = true)]
    public class PurchaseResult
    {
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
    }
}
