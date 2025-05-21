using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IDownloadExcel
    {
        public string DownloadExcelToDevice(BillDetails[] data,string details,string branch);
        public string DownloadExcelPurchaseToDevice(PurchaseDetails[] bills, string details, string branch);
    }
}
