using AutoDynamics.Shared.Services;
using Microsoft.JSInterop;
namespace AutoDynamics.Web.Services
{
    public class WhatsAppService:IWhatsAppService
    {
        //private readonly IJSRuntime _jsRuntime;

        //public WhatsAppService(IJSRuntime jsRuntime)
        //{
        //    _jsRuntime = jsRuntime;
        //}

        public async Task<bool> SendMessage(string phoneNumber, string message)
        {
            //if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
            //{
            //    return false;
            //}

            //var url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

            //await _jsRuntime.InvokeVoidAsync("open", url, "_blank");

            return true;
        }

        public async Task<string> StartWhatsApp() { return ""; }
        public async Task<string> StopWhatsApp() { return ""; }
    }
}
