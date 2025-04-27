using AutoDynamics.Shared.Modals;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IDatabaseHandler
    {
        IDbConnection CreateConnection();

        Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null);
        Task<int[]> InsertBillAsync(Bill bill, List<BillItem> billItems, BillPayment billPayment,bool isUpdating);
        Task<int> InsertPurchaseBillAsync(Purchase purchaseBill, List<PurchaseItems> purchaseItems, bool isUpdating);
        Task<List<BillDetails>> GetAllBillsAsync();
        Task<List<BillDetails>> GetCustomerBillsAsync(string id);
        Task<string> GenerateID(string startsWith, int size, string tableName, string columnName);
    }
}
