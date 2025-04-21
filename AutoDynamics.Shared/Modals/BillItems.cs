using AutoDynamics.Shared.Modals.PurchaseTypes;

namespace AutoDynamics.Shared.Modals
{
    public class BillItem
    {
        public int BillItemID { get; set; }
        public int BillID { get; set; }
        public Bill Bill { get; set; } = null!;
        public string ItemType { get; set; } = "PRODUCT"; // "SERVICE" or "PRODUCT"
        public TaxRate TaxRate { get; set; } = TaxRate.TAX_28;
        public string HSNCode { get; set; } = "";
        public bool itemOutOfStock { get; set; } = false;
        public bool isItemSelected { get; set; } = false;
        public string Branch { get; set; } = "";
        public string ItemID { get; set; } = "";
        public int AvailableQuantity { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TaxableValue { get; set; }
    }

}
