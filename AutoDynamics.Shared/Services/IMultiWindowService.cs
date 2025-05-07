using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IMultiWindowService
    {
        public void OpenBlazorPageInNewWindow(string route, string title);
    }
}
