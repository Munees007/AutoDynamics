using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.PurchaseTypes
{
    public class PurchaseItems
    {
        public long PurchaseItemID { get; set; }  // Primary key
        public long PurchaseBillID { get; set; }  // Foreign key to PurchaseBills table
        public string ProductID { get; set; }     // Foreign key to Product table
        public string ProductName { get; set; }
        public int Quantity { get; set; } = 1;        // Quantity of the product
        public decimal UnitPrice { get; set; }    // Unit price of the product

        // Discount details
        public DiscountType DiscountType { get; set; } = DiscountType.PERCENT;  // Type of discount (PERCENT, AMOUNT, NONE)
        public decimal DiscountValue { get; set; } = 0.00m;  // Discount value (percentage or amount)
        public DiscountScope DiscountScope { get; set; } = DiscountScope.PER_UNIT;  // Scope of discount (PER_UNIT, TOTAL)

        public decimal TaxableValue { get; set; }

        public TaxRate TaxRate { get; set; } = TaxRate.TAX_28;  // Tax rate applicable to the product
        // Final calculated price
        public decimal TotalPrice { get; set; }  // Total price after discount

        // Navigation properties (for ORM like Entity Framework)
        
        public ProductType Product { get; set; }  // Navigation property to the Product table
    }

    public enum DiscountType
    {
        PERCENT,
        AMOUNT,
        NONE
    }

    // Enum for Discount Scope (PER_UNIT, TOTAL)
    public enum DiscountScope
    {
        PER_UNIT,
        TOTAL
    }

    public enum TaxRate
    {
        TAX_28,
        TAX_18
    }
}
