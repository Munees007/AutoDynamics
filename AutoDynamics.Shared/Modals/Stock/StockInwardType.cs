using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Stock
{
    public enum Status
    {
        Pending,
        Received,
        Canceled
    }
    public class StockInwardType
    {
        public int Id { set; get; } = 0;
        public int StockOutwardID { set; get; } = 0;
        public Status status { set; get; } = Status.Pending;
        public string SourceLocation { set; get; } = string.Empty;
        public string DestinationLocation {  set; get; } = string.Empty;
        public StockOutwardType StockOutward { set; get; } = new();
        public string ReceivedBy {  set; get; } = string.Empty;
        public DateTime ReceivedData { get; set; } = DateTime.MinValue;
    }
}
