using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals.ComponentsTypes
{
    public class CustomToastType
    {
        public string message { set; get; } = "";
        public ToastType type { set; get; }

        public bool showToast = false;

        public void ShowToast(string message,ToastType type)
        {
            this.message = message;
            this.type = type;
            showToast = true;
        }
    }

    public enum ToastType
    {
        sucess,
        warning,
        info,
        error
    }
}
