using System;
using System.Collections.Generic;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.BusinessLogic.Services {

    /// <inheritdoc/>
    public class CachePageDataService : ICachePageDataService {

        private Dictionary<Guid, object> cache;

        public CachePageDataService() {
            cache = new Dictionary<Guid, object>();
        }

        /// <inheritdoc/>
        public void Dispose() {
        }

        /// <inheritdoc/>
        public object GetFromStore(Guid storageId) {
            if (!cache.ContainsKey(storageId))
                throw new ArgumentException();

            return cache[storageId];
        }

        /// <inheritdoc/>
        public object RemoveFromStore(Guid storageId) {
            if (cache.ContainsKey(storageId)) {
                var obj = cache[storageId];
                cache.Remove(storageId);
                return obj;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Store(Guid storageId, object o) {
            cache.Add(storageId, o);
        }

        /// <inheritdoc/>
        public void UpdateCachedPageData(Guid storageId, Func<object, object> updateFunc) {
            if (!cache.ContainsKey(storageId))
                throw new ArgumentException();
            
            cache[storageId] = updateFunc(cache[storageId]);
        }
    }
}
