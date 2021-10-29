using System;
using System.Collections.Generic;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.BusinessLogic.Services {
    public class CachePageDataService : ICachePageDataService {

        private Dictionary<Guid, object> cache;

        public CachePageDataService() {
            cache = new Dictionary<Guid, object>();
        }

        public void Dispose() {
        }

        public object GetFromStore(Guid storageId) {
            if (!cache.ContainsKey(storageId))
                throw new ArgumentException();

            return cache[storageId];
        }

        public object RemoveFromStore(Guid storageId) {
            if (cache.ContainsKey(storageId)) {
                var obj = cache[storageId];
                cache.Remove(storageId);
                return obj;
            }

            return null;
        }

        public void Store(Guid storageId, object o) {
            cache.Add(storageId, o);
        }

        public void UpdateCachedPageData(Guid storageId, Func<object, object> updateFunc) {
            if (!cache.ContainsKey(storageId))
                throw new ArgumentException();
            
            cache[storageId] = updateFunc(cache[storageId]);
        }
    }
}
