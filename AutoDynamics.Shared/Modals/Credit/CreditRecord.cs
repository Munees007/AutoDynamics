using AutoDynamics.Shared.Modals.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Credit
{
    public class CreditRecord
    {
        public ulong CreditID { get; set; }
        public string CustomerID { get; set; }
        public UserModal Customer { get; set; }

        public string BillNo { get; set; }
        public ulong? BillID { get; set; }
        public string Branch { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal? RemainingBalance { get; set; } = 0m;
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string[] GenerateForPDF()
        {
            return (new string[] {this.Customer.Name, this.Customer.Contact, this.CreditAmount.ToString("F2"), this.PaidAmount.ToString("F2"), this.RemainingBalance?.ToString("F2")!});
        }
    }
}
