using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Accounts
{
    public class CustomerStatement
    {
        public DateTime date { get; set; }
        public string accountType { get; set; } = string.Empty;
        public int EntryID;
        public string type { get; set; } = string.Empty;
        public string particulars { get; set; } = string.Empty;
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }
}
