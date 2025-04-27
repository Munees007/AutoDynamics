using AutoDynamics.Shared.Modals;
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
    }
}
