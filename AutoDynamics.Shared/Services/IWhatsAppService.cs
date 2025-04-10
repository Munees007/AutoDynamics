using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public interface IWhatsAppService
    {
        Task<string> StartWhatsApp();
        Task<bool> SendMessage(string phoneNumber, string message);
        Task<string> StopWhatsApp();
    }
}
