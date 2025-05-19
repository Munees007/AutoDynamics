using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Modals.Stock;
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
        Task<List<BillDetails>> GetFilteredBillsAsync(BillDateFilterType filterType, DateTime? startDate = null, DateTime? endDate = null, DateTime? customMonthYear = null);
        Task<List<BillDetails>> GetCustomerBillsAsync(string id);
        Task<string> GenerateID(string startsWith, int size, string tableName, string columnName);

        Task<int> InsertStockOutwardAsync(StockOutwardType outward, List<Outward> outwardItems);
        Task<List<StockOutwardType>> GetStockOutward();
        Task UpdateStockOutwardAsync(int stockOutwardId, StockOutwardType outward, List<Outward> outwardItems);
        Task<List<StockInwardType>> GetStockInwards();
        Task AcceptInward(StockInwardType stockInward);
    }
}
