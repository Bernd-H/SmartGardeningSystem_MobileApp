using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;

namespace MobileApp.BusinessLogic.Services {
    public class ModuleDataStore : IDataStore<ModuleInfoDto> {

        private List<ModuleInfoDto> modules;


        private ILogger Logger;

        private IAPIManager _APIManager;

        public ModuleDataStore(ILoggerService loggerService, IAPIManager apiManager) {
            Logger = loggerService.GetLogger<ModuleDataStore>();
            _APIManager = apiManager;

            modules = new List<ModuleInfoDto>();
        }

        public Task<bool> AddItemAsync(ModuleInfoDto item) {
            modules.Add(item);

            // update api
            throw new NotImplementedException();

            return Task.FromResult(true);
        }

        public Task<bool> DeleteItemAsync(string id) {
            var moduleToDelete = modules.Find(m => m.Id.ToString() == id);
            if (moduleToDelete != null) {
                modules.Remove(moduleToDelete);

                // update api
                throw new NotImplementedException();

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<ModuleInfoDto> GetItemAsync(string id) {
            var module = modules.Find(m => m.Id.ToString() == id);

            return Task.FromResult(module);
        }

        public async Task<IEnumerable<ModuleInfoDto>> GetItemsAsync(bool forceRefresh = false) {
            if (forceRefresh || modules.Count == 0) {
                // request modules
                var _modules = await _APIManager.GetModules();
                if (_modules != null)
                    modules = _modules.ToList();
            }

            return modules;
        }

        public Task<bool> UpdateItemAsync(ModuleInfoDto item) {
            throw new NotImplementedException();
        }
    }
}
