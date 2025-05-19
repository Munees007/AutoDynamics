using AutoDynamics.Shared.Modals.Billing;
using Microsoft.JSInterop;
namespace AutoDynamics.Shared.Services
{
    public interface IPDFGenerator
    {
        Task<string> GeneratePdfAsync(BillDetails billDetails,IJSRuntime js);
    }
}
