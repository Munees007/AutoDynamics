using AutoDynamics.Shared.Modals.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Credit
{
    public class SupplierCreditRecord
    {
        public ulong SupplierCreditID { get; set; }
        public string SupplierID { get; set; }
        public Supplier Supplier { get; set; }
        public string PurchaseBillNo { get; set; }
        public string Invoice { get; set; }
        public ulong? PurchaseBillID { get; set; }
        public string Branch { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal? RemainingBalance { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
