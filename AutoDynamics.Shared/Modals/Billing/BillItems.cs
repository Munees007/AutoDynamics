using AutoDynamics.Shared.Modals.Product;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Pages.Components;

namespace AutoDynamics.Shared.Modals.Billing
{
    public class BillItem
    {

        //field for PurchaseItems

        public SearchSelect<ProductType> productListRef { set; get; }
        public SearchSelect<BrandType> brandListRef { set; get; }
        public int index { set; get; }
        public bool showSuggestions { get; set; } = false;
        public bool showBrandSuggestions { get; set; } = false;

        public string ProductID { get; set; }     // Foreign key to Product table
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string BrandID { get; set; }
        public ProductType Product { get; set; }  // Navigation property to the Product table
        public BrandType Brand { get; set; }


        //field for BillItem and Service
        public int BillItemID { get; set; }
        public int BillID { get; set; }
        public Bill Bill { get; set; } = null!;
        public string ItemType { get; set; } = "PRODUCT"; // "SERVICE" or "PRODUCT"
        public TaxRate TaxRate { get; set; } = TaxRate.TAX_28;
        public string HSNCode { get; set; } = "";
        public bool itemOutOfStock { get; set; } = false;

        public bool ProductIDValid = false;
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
