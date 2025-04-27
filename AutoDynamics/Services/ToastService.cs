using AutoDynamics.Shared.Modals.ComponentsTypes;
using AutoDynamics.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Services
{
    public class ToastService:IToastService
    {
        public CustomToastType toast { set; get; } = new();
        public event Action OnToastUpdated;
        public void ShowToast(string message, ToastType type)
        {
            toast = new CustomToastType
            {
                message = message,
                type = type,
                showToast = true
            };

            OnToastUpdated?.Invoke();

        }
    }
}
