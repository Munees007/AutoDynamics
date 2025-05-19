using AutoDynamics.Shared.Modals.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.PurchaseTypes
{
    public class Purchase
    {
        public long PurchaseBillID { get; set; }

        public string SupplierID { get; set; } = string.Empty;
        public Supplier Supplier { get; set; } = null!;

        public PaymentType type { get; set; } = PaymentType.CASH;

        public TaxType taxType { get; set; } = TaxType.EXCLUSIVE_TAX;

        public string InvoiceNumber { get; set; } = "";

        public string Branch { get; set; } = string.Empty;
        public int BillingYear { get; set; }
        public int BillNo { get; set; } // Resets every year per branch

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; } = 0.00m;         // Sum of item totals before discount
        public decimal DiscountAmount { get; set; } = 0.00m;
        public decimal GrandTotal { get; set; } = 0.00m;           // TotalAmount - DiscountAmount + Tax

        public string Notes { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PurchaseItems> PurchaseItems { get; set; } = new List<PurchaseItems>();
        
    }

    public enum PaymentType
    {
        CASH,
        CREDIT
    }

    public enum TaxType
    {
        INCLUSIVE_TAX,
        EXCLUSIVE_TAX
    }
}
