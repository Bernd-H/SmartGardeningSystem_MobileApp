using System;

namespace MobileApp.Common.Specifications.Services {

    /// <summary>
    /// Class to cache objects associated with a Guid.
    /// </summary>
    public interface ICachePageDataService : IDisposable {

        /// <summary>
        /// Stores an object and it's id in memory.
        /// </summary>
        /// <param name="storageId">Id of the object.</param>
        /// <param name="o">The object to store.</param>
        void Store(Guid storageId, object o);

        /// <summary>
        /// Returns and removes an object form storage.
        /// </summary>
        /// <param name="storageId">Id of the object.</param>
        /// <returns>Returns the object that was associated to the given <paramref name="storageId"/>.</returns>
        object RemoveFromStore(Guid storageId);

        /// <summary>
        /// Updates a already stored object.
        /// </summary>
        /// <param name="storageId">Id of the object that should get updated.</param>
        /// <param name="updateFunc">Takes the current object stored at <paramref>storageId</paramref> and returns the updated object.</param>
        /// <exception cref="ArgumentException">When there is no object stored at the passed <paramref>storageId</paramref>.</exception>
        void UpdateCachedPageData(Guid storageId, Func<object, object> updateFunc);

        /// <summary>
        /// Gets an object without removing it.
        /// </summary>
        /// <param name="storageId">Id of the object.</param>
        /// <returns>An object that is associated with the given <paramref name="storageId"/>.</returns>
        /// <exception cref="ArgumentException">When there is no object stored at the passed <paramref>storageId</paramref>.</exception>
        object GetFromStore(Guid storageId);
    }
}
