using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.Billing
{
    public enum BillDateFilterType
    {
        All,
        Today,
        Yesterday,
        ThisMonth,
        ThisYear,
        CustomDate,
        CustomRange,
        CustomMonthYear,
    }
}
