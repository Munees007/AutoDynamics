using AutoDynamics.Shared.Modals.Accounts;
using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.Customer;
using Microsoft.JSInterop;
namespace AutoDynamics.Shared.Services
{
    public interface IPDFGenerator
    {
        Task<string> GeneratePdfAsync(BillDetails billDetails,IJSRuntime js);
        Task<string> GenerateCustomerStatement(List<CustomerStatement> customerStatements,decimal opening,UserModal customer);
    }
}
