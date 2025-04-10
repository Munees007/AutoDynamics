namespace AutoDynamics.Shared.Services
{
    public interface IAlertService
    {
        Task ShowAlertAsync(string title, string message, string buttonText = "OK");
    }
}
