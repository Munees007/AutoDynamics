using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.PurchaseTypes
{
    public class PurchaseDetails
    {
        public Purchase purchase { get; set; }
        public List<PurchaseItems> purchaseItems { get; set; } = new List<PurchaseItems>();
    }
}
