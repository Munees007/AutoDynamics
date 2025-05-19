using AutoDynamics.Shared.Modals.Customer;
using AutoDynamics.Shared.Pages.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Accounts.Recipt
{
    public class CreditReciptType
    {
        public DateTime ReciptDate { get; set; }
        public UserModal customer { get; set; } = new();
        public List<CreditBill> creditBills { get; set; } = new();
    }

    public class CreditBill
    {
        public int index { get; set; } = 0;
        public int billId { get; set; } = 0;
        public int billNo { get; set; } = 0;
        public string branch { get; set; } = "";
        public DateTime BillDate { get; set; }
        public SearchSelect<CreditBill> creditListRef { get; set; }
        public bool showSuggestion { get; set; } = false;
        public PaymentTypes paymentType { get; set; } = PaymentTypes.CASH;
        public decimal dueAmount { get; set; } = 0;
        public decimal amountPayed { get; set; } = 0;
    }

}
