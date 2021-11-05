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

        private IAPIManager APIManager;

        public ModuleDataStore(ILoggerService loggerService, IAPIManager apiManager) {
            Logger = loggerService.GetLogger<ModuleDataStore>();
            APIManager = apiManager;

            modules = new List<ModuleInfoDto>();
        }

        public async Task<bool> AddItemAsync(ModuleInfoDto item) {
            // update api
            bool success = await APIManager.AddModule(new Common.Models.Entities.ModuleInfo {
                Id = item.Id,
                ModuleTyp = item.Type.Value,
                Name = item.Name,
                AssociatedModules = item.CorrespondingValves
            });

            if (success) {
                modules.Add(item);
            }

            return success;
        }

        public async Task<bool> DeleteItemAsync(string id) {
            var moduleToDelete = modules.Find(m => m.Id.ToString() == id);
            if (moduleToDelete != null) {
                // update api
                bool success = await APIManager.DeleteModule(moduleToDelete.Id);

                // remove local version and remove all linked entries in other modules
                if (success) {
                    modules.Remove(moduleToDelete);
                    foreach (var module in modules) {
                        if (module.CorrespondingValves?.Contains(moduleToDelete.Id) ?? false) {
                            var valvesList = module.CorrespondingValves.ToList();
                            valvesList.Remove(moduleToDelete.Id);
                            module.CorrespondingValves = valvesList;
                        }
                    }
                }

                return success;
            }

            return false;
        }

        public Task<ModuleInfoDto> GetItemAsync(string id) {
            var module = modules.Find(m => m.Id.ToString() == id);

            return Task.FromResult(module);
        }

        public async Task<IEnumerable<ModuleInfoDto>> GetItemsAsync(bool forceRefresh = false) {
            if (forceRefresh || modules.Count == 0) {
                // request modules
                var _modules = await APIManager.GetModules();
                if (_modules != null)
                    modules = _modules.ToList();
            }

            return modules;
        }

        public async Task<bool> UpdateItemAsync(ModuleInfoDto item) {
            bool success = await APIManager.UpdateModule(new Common.Models.Entities.ModuleInfo {
                Id =  item.Id,
                ModuleTyp = item.Type.Value,
                Name = item.Name,
                AssociatedModules = item.CorrespondingValves
            });

            // update stored version
            if (success) {
                modules.Remove(modules.Find(m => m.Id == item.Id));
                modules.Add(item);
            }

            return success;
        }
    }
}
