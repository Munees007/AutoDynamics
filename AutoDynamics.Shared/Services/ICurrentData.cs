using AutoDynamics.Shared.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface ICurrentData
    {
        public object currentCustomer { get; set; }
        public object GetCurrentCustomer();
        public void SetCurrentCustomer(object currentData);
    }
}
