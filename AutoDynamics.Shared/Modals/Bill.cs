

namespace AutoDynamics.Shared.Modals
{
    public class Bill
    {
        public int BillID { get; set; }
        public string Branch { get; set; } = string.Empty; // Branch identifier
        public int BillingYear { get; set; } // Year to reset BillNo
        public int BillNo { get; set; } // Resets every year per branch
        public string CustomerID { get; set; }
        public string UsageReading { get; set; } = "";
        public UserModal Customer { get; set; } = null!;
        public string? VehicleNo { get; set; }
        public VehicleType? Vehicle { get; set; }
        public DateTime BillDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal Discount { get; set; } = 0.00m;
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
    }

}
