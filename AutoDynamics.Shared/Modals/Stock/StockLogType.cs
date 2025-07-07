using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDynamics.Shared.Modals.Product;
namespace AutoDynamics.Shared.Modals.Stock
{
    public enum ActionType
    {
        INSERT,UPDATE
    }
    public class StockLogType
    {
        public string ProductID { get; set; } = string.Empty;
        public ProductType Product { get; set; } = new();
        public string Branch { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public ActionType Action { get; set; }
        public int OldQuantity { get; set; } = 0;
        public int NewQuantity { get; set; } = 0;
        public DateTime CreatedAt { get; set; }

    }
}
