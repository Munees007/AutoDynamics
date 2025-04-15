using AutoDynamics.Shared.Modals;
using AutoDynamics.Shared.Services;

namespace AutoDynamics.Web.Services
{
    class CurrentData : ICurrentData
    {
        private readonly Dictionary<CurrentType, object> _dataStore = new();

        public void Set(CurrentType type, object data)
        {
            _dataStore[type] = data;
        }

        public T? Get<T>(CurrentType type) where T : class
        {
            if (_dataStore.TryGetValue(type, out var value))
                return value as T;
            return null;
        }

        public void Clear(CurrentType type)
        {
            _dataStore.Remove(type);
        }

        public void ClearAll()
        {
            _dataStore.Clear();
        }
    }
}
