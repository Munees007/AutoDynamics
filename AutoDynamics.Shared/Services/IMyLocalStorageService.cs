namespace AutoDynamics.Shared.Services
{
    public interface IMyLocalStorageService
    {
        Task SetItemAsync(string key, string value);
        Task<string?> GetItemAsync(string key);
        Task RemoveItemAsync(string key);

        Task ClearAsync();
    }
}
