using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Accounts
{
    
    public enum TransactionType
    {
        RECEIPT,
        PAYMENT,
        BILL,
        PURCHASE,
        EXPENSE,
        OTHER
    }
    public class Ledger
    {
        public int LedgerID { get; set; } = 0;
        public int EntryID { get; set; } = 0;
        public string ForWho { get; set; } = string.Empty;
        public string billOrInvoiceNo { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public int AccountID { get; set; }
        public LedgerAcccounts AccountType { get; set; } = new LedgerAcccounts();
        public string Branch { get; set; } = "";
        public TransactionType TransactionType { get; set; } = TransactionType.OTHER;
        public int ReferenceID { get; set; } = 0;
        public string Particulars { get; set; } = "";
        public decimal CR_Amount { get; set; } = 0m;
        public decimal DR_Amount { get; set; } = 0m;
        public decimal Balance { get; set; } = 0m;
    }
}
