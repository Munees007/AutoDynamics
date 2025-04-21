using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals
{
    public class StockType
    {
        public string ProductID { get; set; } = "";
        public ProductType Product { get; set; } = new();
        public int AvailableQuantity { get; set; } = 0;
        public string Branch { get; set; } = "";
    }
}
