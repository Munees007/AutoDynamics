namespace AutoDynamics.Shared.Modals
{
    public class BillDetails
    {
        public Bill Bill { get; set; }
        public List<BillItem> BillItems { get; set; } = new List<BillItem>();
        public BillPayment BillPayment { get; set; }

        public UserModal customer { get; set; }
    }

}
