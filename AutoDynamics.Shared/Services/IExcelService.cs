using AutoDynamics.Shared.Modals.Billing;
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
    }
}
