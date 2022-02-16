using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Services {

    /// <summary>
    /// Class to administrate objects. (repository)
    /// </summary>
    /// <typeparam name="T">Type of the objects that this class stores.</typeparam>
    public interface IDataStore<T> {

        /// <summary>
        /// Adds an item to the repository.
        /// </summary>
        /// <param name="item">Item to add.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> AddItemAsync(T item);

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        /// <param name="item">Updated item.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> UpdateItemAsync(T item);

        /// <summary>
        /// Removes an existing item from the repository.
        /// </summary>
        /// <typeparam name="T1">Type of the Id of the item.</typeparam>
        /// <param name="id">Id of the item that should get removed.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        Task<bool> DeleteItemAsync<T1>(T1 id) where T1 : struct; // = C# 8.0: notnull

        /// <summary>
        /// Gets an item with a specific <paramref name="id"/> from the repository.
        /// </summary>
        /// <typeparam name="T1">Type of the Id of the item.</typeparam>
        /// <param name="id">Id of the item.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains the item.
        /// </returns>
        Task<T> GetItemAsync<T1>(T1 id) where T1 : struct;

        /// <summary>
        /// Gets all items from the repository.
        /// </summary>
        /// <param name="forceRefresh">True to load all items from their source and not from the cache. (Updates the internal cache)</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains all items of the repository.
        /// </returns>
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
    }
}
