using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using NLog;

namespace MobileApp.BusinessLogic.Services {

    /// <inheritdoc/>
    public class ModuleDataStore : IDataStore<ModuleInfo> {

        private List<ModuleInfo> modules;


        private ILogger Logger;

        private IAPIManager APIManager;

        public ModuleDataStore(ILoggerService loggerService, IAPIManager apiManager) {
            Logger = loggerService.GetLogger<ModuleDataStore>();
            APIManager = apiManager;

            modules = new List<ModuleInfo>();
        }

        /// <inheritdoc/>
        public async Task<bool> AddItemAsync(ModuleInfo item) {
            Logger.Info($"[AddItemAsync]Trying to add a new module.");

            // update api
            bool success = await APIManager.AddModule(item);

            if (success) {
                modules.Add(item);
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteItemAsync<T1>(T1 id) where T1 : struct {
            Logger.Info($"[DeleteItemAsync]Deleting module with id={id}.");

            byte moduleId = Utils.ConvertValue<byte, T1>(id);

            var moduleToDelete = modules.Find(m => m.ModuleId == moduleId);
            if (moduleToDelete != null) {
                // update api
                bool success = await APIManager.DeleteModule(moduleToDelete.ModuleId);

                // remove local version and remove all linked entries in other modules
                if (success) {
                    modules.Remove(moduleToDelete);
                    foreach (var module in modules) {
                        if (module.AssociatedModules?.Contains(moduleToDelete.ModuleId) ?? false) {
                            var valvesList = module.AssociatedModules.ToList();
                            valvesList.Remove(moduleToDelete.ModuleId);
                            module.AssociatedModules = valvesList;
                        }
                    }
                }

                return success;
            }

            return false;
        }

        /// <inheritdoc/>
        public Task<ModuleInfo> GetItemAsync<T1>(T1 id) where T1 : struct {
            byte moduleId = Utils.ConvertValue<byte, T1>(id);

            Logger.Info($"[GetItemAsync]Requested module with id={Utils.ConvertByteToHex(moduleId)} from local memory.");
            var module = modules.Find(m => m.ModuleId == moduleId);

            return Task.FromResult(module);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ModuleInfo>> GetItemsAsync(bool forceRefresh = false) {
            if (forceRefresh || modules.Count == 0) {
                Logger.Info($"[GetItemsAsync]Requesting all modules from basestation.");

                // request modules
                var _modules = await APIManager.GetModules();
                if (_modules != null)
                    modules = _modules.ToList();
            }
            else {
                Logger.Trace($"[GetItemsAsync]Returning modules from local cache.");
            }

            return modules;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateItemAsync(ModuleInfo item) {
            Logger.Info($"[UpdateItemAsync]Updating module with id={Utils.ConvertByteToHex(item.ModuleId)}.");
            bool success = await APIManager.UpdateModule(item);

            // update stored version
            if (success) {
                modules.Remove(modules.Find(m => m.ModuleId == item.ModuleId));
                modules.Add(item);
            }

            return success;
        }
    }
}
