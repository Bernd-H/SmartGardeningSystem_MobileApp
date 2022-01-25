using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);

        Task<bool> UpdateItemAsync(T item);

        Task<bool> DeleteItemAsync<T1>(T1 id) where T1 : struct; // = C# 8.0: notnull

        Task<T> GetItemAsync<T1>(T1 id) where T1 : struct;

        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
    }
}
