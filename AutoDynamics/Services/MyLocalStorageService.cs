using AutoDynamics.Shared.Services;

namespace AutoDynamics.Services
{
    class MyLocalStorageService : IMyLocalStorageService
    {
        public Task SetItemAsync(string key, string value)
        {
            
            Preferences.Set(key, value);
            return Task.CompletedTask;
        }

        public Task<string?> GetItemAsync(string key)
        {
            return Task.FromResult(Preferences.Get(key, null));
        }

        public Task RemoveItemAsync(string key)
        {
            Preferences.Remove(key);
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            Preferences.Clear();
            return Task.CompletedTask;
        }
    }
}
