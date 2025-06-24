using AutoDynamics.Shared.Modals.Credit;
using AutoDynamics.Shared.Modals.Customer;
using AutoDynamics.Shared.Pages.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Accounts.Recipt
{
    public class PaymentReciptType
    {
        public int PaymentId { get; set; }
        public string Branch { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public Supplier supplier { get; set; } = new();
        public decimal TotalAmountPaid { get; set; } = new();
        public List<PaymentBill> paymentBills { get; set; } = new();
        public string Narration { get; set; } = string.Empty;
        public string CheckNumber { get; set; } = string.Empty;
        public int BillingYear { get; set; } = 0;
        public PaymentTypes paymentType { get; set; }
        public int PaymentNo { get; set; } = 0;
    }

    

    public class PaymentBill
    {
        public int index { get; set; } = 0;
        public int paymentId { get; set; } = 0;
        public int purchaseBillID { get; set; } = 0;
        public string invoice { get; set; } = string.Empty;
        public int supplierCreditId { get; set; } = 0;
        public string branch { get; set; } = "";
        public DateTime purchaseDate { get; set; } = DateTime.Now;
        public SearchSelect<PaymentBill> creditListRef { get; set; }
        public bool showSuggestion { get; set; } = false;
        public CreditStatus CreditStatus { get; set; }
        public PaymentTypes paymentType { get; set; } = PaymentTypes.CASH;
        public decimal dueAmount { get; set; } = 0;
        public decimal paidAmount { get; set; } = 0;
        public decimal remainingBalance { get; set; } = 0;
        public decimal amountPayed { get; set; } = 0;

        public int purchaseNo { get; set; } = 0;

        public bool isSelected { get; set; } = true;

        public bool CheckAmount()
        {
            if (amountPayed > 0 && amountPayed <= dueAmount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
