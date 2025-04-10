using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals
{
    public class BillPayment
    {
        public int PaymentID { get; set; }
        public int BillID { get; set; }
        public Bill Bill { get; set; } = null!;
        public decimal CashAmount { get; set; } = 0.00m;
        public decimal BankAmount { get; set; } = 0.00m;
        public decimal CardAmount { get; set; } = 0.00m;
        public decimal UPIAmount { get; set; } = 0.00m;
        public DateTime PaidAt { get; set; } = DateTime.Now;

        public decimal TotalPaid => CashAmount + BankAmount + CardAmount + UPIAmount; // Computed in .NET
    }

}
