using AutoDynamics.Shared.Services;
using Blazored.LocalStorage;
namespace AutoDynamics.Web.Services
{
    public class MyLocalStorageServices: IMyLocalStorageService
    {
        private readonly ILocalStorageService _localStorage;

        public MyLocalStorageServices(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SetItemAsync(string key, string value)
        {
            await _localStorage.SetItemAsync(key, value);
        }

        public async Task<string?> GetItemAsync(string key)
        {
            return await _localStorage.GetItemAsync<string>(key);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _localStorage.RemoveItemAsync(key);
        }

        public async Task ClearAsync()
        {
            await _localStorage.ClearAsync();
        }
    }
}
