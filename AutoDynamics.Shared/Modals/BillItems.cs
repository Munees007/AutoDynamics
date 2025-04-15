namespace AutoDynamics.Shared.Modals
{
    public class BillItem
    {
        public int BillItemID { get; set; }
        public int BillID { get; set; }
        public Bill Bill { get; set; } = null!;
        public string ItemType { get; set; } = "PRODUCT"; // "SERVICE" or "PRODUCT"
        public string ItemID { get; set; } = "";
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TaxableValue { get; set; }
    }

}
