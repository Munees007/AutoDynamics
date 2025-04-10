using AutoDynamics.Shared.Modals;
using AutoDynamics.Shared.Services;

namespace AutoDynamics.Services
{
    class CurrentData:ICurrentData
    {
        public object currentCustomer { get; set; } = new object();
        public object GetCurrentCustomer()
        {
            return currentCustomer;
        }
        public void SetCurrentCustomer(object data)
        {
            this.currentCustomer = data;
        }
    }
}
