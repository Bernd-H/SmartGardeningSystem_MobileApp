using System;

namespace MobileApp.Common.Specifications.Services {
    public interface ICachePageDataService : IDisposable {

        void Store(Guid storageId, object o);

        /// <summary>
        /// Returns and removes an object form storage.
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns>Returns the stored object</returns>
        object RemoveFromStore(Guid storageId);

        /// <summary>
        /// Updates a already stored object.
        /// </summary>
        /// <param name="storageId">Id of the object that should get updated.</param>
        /// <param name="updateFunc">Takes the current object stored at <paramref>storageId</paramref> and returns the updated object.</param>
        /// <exception cref="ArgumentException">When there is no object stored at the passed <paramref>storageId</paramref>.</exception>
        void UpdateCachedPageData(Guid storageId, Func<object, object> updateFunc);

        /// <param name="storageId"></param>
        /// <returns>An object stored under <paramref>storageId</paramref>.</returns>
        /// <exception cref="ArgumentException">When there is no object stored at the passed <paramref>storageId</paramref>.</exception>
        object GetFromStore(Guid storageId);
    }
}
