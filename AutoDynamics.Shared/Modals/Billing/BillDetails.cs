using AutoDynamics.Shared.Modals.Customer;

namespace AutoDynamics.Shared.Modals.Billing
{
    public class BillDetails
    {
        public Bill Bill { get; set; }
        public List<BillItem> BillItems { get; set; } = new List<BillItem>();
        public BillPayment BillPayment { get; set; }

        public UserModal customer { get; set; }
    }

}
