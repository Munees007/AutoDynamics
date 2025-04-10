using AutoDynamics.Shared.Services;
using Microsoft.JSInterop;
namespace AutoDynamics.Web.Services
{
    public class AlertService:IAlertService
    {
        private readonly IJSRuntime _jsRuntime;

        public AlertService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
        {
            await _jsRuntime.InvokeVoidAsync("alert", $"{title}\n\n{message}");
        }
    }
}
