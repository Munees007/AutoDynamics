using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.Credit;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Modals.Stock;
using Microsoft.AspNetCore.Components.Forms;
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
        public Task<List<List<string>>> readData(IBrowserFile file,int workSheetPosition,int startRow);
        public Task<string> CreateCreditExcel(string branch, List<CreditRecord> sivakasiCredit = null, List<CreditRecord> bypassCredit = null);
        public Task<string> CreateStockExcel(string branch, List<StockType> sivakasiCredit = null, List<StockType> bypassCredit = null);
    }
}
