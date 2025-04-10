using AutoDynamics.Shared.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
namespace AutoDynamics.Services
{
    class AlertService : IAlertService
    {
        public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
        {
            await Application.Current.MainPage.DisplayAlert(title, message, buttonText);
        }
    }
}
