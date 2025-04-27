using AutoDynamics.Shared.Modals.ComponentsTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IToastService
    {
        public CustomToastType toast { set; get; }
        public event Action OnToastUpdated;
        public void ShowToast(string message, ToastType type);
    }
}
