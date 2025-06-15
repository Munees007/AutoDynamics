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
        public int ReceiptId { get; set; }
        public int ReceiptNO { get; set; }

        public string CheckNumber { get; set; } = string.Empty;

        public string narration {get;set;} = string.Empty;

        public int BillingYear { get; set; }
        public string Branch { get; set; }
        public DateTime ReciptDate { get; set; } = DateTime.Now;
        public UserModal customer { get; set; } = new();
        public decimal TotalAmountPaid { get; set; } = new();
        public List<CreditBill> creditBills { get; set; } = new();
    }

    public enum CreditStatus
    {
        Pending,
        Partial,
        Paid
    }

    public class CreditBill
    {
        public int index { get; set; } = 0;
        public int creditId { get; set; } = 0;
        public int billId { get; set; } = 0;
        public int billNo { get; set; } = 0;
        public string branch { get; set; } = "";
        public DateTime BillDate { get; set; } = DateTime.Now;
        public SearchSelect<CreditBill> creditListRef { get; set; }
        public bool showSuggestion { get; set; } = false;
        public CreditStatus CreditStatus { get; set; }
        public PaymentTypes paymentType { get; set; } = PaymentTypes.CASH;
        public decimal dueAmount { get; set; } = 0;
        public decimal paidAmount { get; set; } = 0;
        public decimal remainingBalance { get; set; } = 0;
        public decimal amountPayed { get; set; } = 0;



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
