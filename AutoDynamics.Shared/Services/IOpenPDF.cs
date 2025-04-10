using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IOpenPDF
    {
        public void OpenPDF(byte[] bytes,JSRuntime js);
    }
}
