using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IExcelService
    {
        public byte[] GenerateExcel(BillDetails[] bills,string details);
        public byte[] GenerateExcelPurchase(PurchaseDetails[] purchaseBills, string details);
    }
}
