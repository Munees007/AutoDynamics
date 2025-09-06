namespace AutoDynamics.Shared.Services
{
    public interface IAlertService
    {
        Task<bool> ShowAlertAsync(string title, string message, string buttonText = "OK");
    }
}
