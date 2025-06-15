using AutoDynamics.Shared.Modals.Accounts;
using AutoDynamics.Shared.Modals.Accounts.Recipt;
using AutoDynamics.Shared.Modals.Billing;
using AutoDynamics.Shared.Modals.Credit;
using AutoDynamics.Shared.Modals.Customer;
using Microsoft.JSInterop;
namespace AutoDynamics.Shared.Services
{
    public interface IPDFGenerator
    {
        Task<string> GeneratePdfAsync(BillDetails billDetails,IJSRuntime js);
        Task<string> CreateReceiptPDF(CreditReciptType creditRecipt, IJSRuntime js);
        Task<string> CreatePaymentPDF(PaymentReciptType creditRecipt, IJSRuntime js);
        Task<string> CreateCreditRecordPDF(IJSRuntime js, string Branch, List<CreditRecord>? sivakasiCredit = null, List<CreditRecord>? bypassCredit = null);
        Task<string> GenerateCustomerStatement(List<CustomerStatement> customerStatements,decimal opening,UserModal customer);
    }
}
