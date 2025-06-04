using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Accounts
{
    public enum LedgerAccountsType
    {
        ASSET,
        LIABILITY,
        INCOME,
        EXPENSE,
        EQUITY
    }
    public class LedgerAcccounts
    {
        public int AcccountId { get; set; }
        public string AccountName { get; set; } = "";
        public LedgerAccountsType AccountType { get; set; }
        public bool isActive { get; set; }
    }
}
