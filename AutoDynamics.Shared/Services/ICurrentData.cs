using AutoDynamics.Shared.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Services
{
    public enum CurrentType
    {
        Customer,
        Supplier,
        Vehicle,
        Bill,
        Purchase,
        CreditRecord,
        Brand,
        Model,
        Product
    }

    public interface ICurrentData
    {
        void Set(CurrentType type, object data);
        T? Get<T>(CurrentType type) where T : class;
        void Clear(CurrentType type);
        void ClearAll();
    }
}
