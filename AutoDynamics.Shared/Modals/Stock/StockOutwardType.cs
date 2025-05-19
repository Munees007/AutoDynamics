using AutoDynamics.Shared.Modals.Product;
using AutoDynamics.Shared.Pages.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Stock
{
    public class Outward
    {
        //fields for Search Component
        public int index = 0;
        public SearchSelect<BrandType> BrandListRef { get; set; }
        public SearchSelect<ProductType> ProductListRef { get; set; }
        public bool showBrandSuggestions { get; set; } = false;
        public bool showProductSuggestions { get; set; } = false;
        public bool isItemSelected { get; set; } = false;
        public bool itemOutOfStock { get; set; } = true;
        //Class fields
        public string ProductID { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; } = 0;
        public string BrandName { get; set; } = string.Empty;
        public string BrandID { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;
        public bool ProductIDValid { get; set; } = false;
        public ProductType Product { get; set; } = new();
        public int Quantity { get; set; } = 0;

    }
    public class StockOutwardType
    {
        public int Id { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public List<Outward> Outwards { get; set; } = new();
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
