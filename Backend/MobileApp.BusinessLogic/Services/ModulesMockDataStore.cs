using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.BusinessLogic.Services {
    //public class ModulesMockDataStore : IDataStore<ModuleInfoDto> {
    //    readonly List<ModuleInfoDto> modules;

    //    public ModulesMockDataStore() {
    //        var id1 = Guid.NewGuid();
    //        var id2 = Guid.NewGuid();
    //        var id3 = Guid.NewGuid();
    //        var id4 = Guid.NewGuid();

    //        modules = new List<ModuleInfoDto>()
    //        {
    //            new ModuleInfoDto {
    //                Id = Guid.NewGuid(),
    //                Type = new ModuleTypes(ModuleTypes.MAINSTATION),
    //                Name = "Hauptstation"
    //            },
    //            new ModuleInfoDto {
    //                Id = Guid.NewGuid(),
    //                Type = new ModuleTypes(ModuleTypes.SENSOR),
    //                Name = "Sensor1",
    //                CorrespondingValves = new List<Guid>() {id1, id2}
    //            },
    //            new ModuleInfoDto {
    //                Id = Guid.NewGuid(),
    //                Type = new ModuleTypes(ModuleTypes.SENSOR),
    //                Name = "Sensor2",
    //                CorrespondingValves = new List<Guid>() {id3, id4}
    //            },
    //            new ModuleInfoDto {
    //                Id = id1,
    //                Type = new ModuleTypes(ModuleTypes.VALVE),
    //                Name = "Valve1"
    //            },
    //            new ModuleInfoDto {
    //                Id = id2,
    //                Type = new ModuleTypes(ModuleTypes.VALVE),
    //                Name = "Valve2"
    //            },
    //            new ModuleInfoDto {
    //                Id = id3,
    //                Type = new ModuleTypes(ModuleTypes.VALVE),
    //                Name = "Valve3"
    //            },
    //            new ModuleInfoDto {
    //                Id = id4,
    //                Type = new ModuleTypes(ModuleTypes.VALVE),
    //                Name = "Valve4"
    //            }
    //        };
    //    }

    //    public async Task<bool> AddItemAsync(ModuleInfoDto item) {
    //        modules.Add(item);

    //        return await Task.FromResult(true);
    //    }

    //    public async Task<bool> UpdateItemAsync(ModuleInfoDto item) {
    //        var oldItem = modules.Where((ModuleInfoDto arg) => arg.Id == item.Id).FirstOrDefault();
    //        modules.Remove(oldItem);
    //        modules.Add(item);

    //        return await Task.FromResult(true);
    //    }

    //    public async Task<bool> DeleteItemAsync(string id) {
    //        var moduleToRemove = modules.Where((ModuleInfoDto arg) => arg.Id.ToString() == id).FirstOrDefault();
    //        modules.Remove(moduleToRemove);

    //        // delete entries in other modules
    //        foreach (var module in modules) {
    //            if (module.CorrespondingValves?.Contains(moduleToRemove.Id) ?? false) {
    //                var valvesList = module.CorrespondingValves.ToList();
    //                valvesList.Remove(moduleToRemove.Id);
    //                module.CorrespondingValves = valvesList;
    //            }
    //        }

    //        return await Task.FromResult(true);
    //    }

    //    public async Task<ModuleInfoDto> GetItemAsync(string id) {
    //        return await Task.FromResult(modules.FirstOrDefault(s => s.Id.ToString() == id));
    //    }

    //    public async Task<IEnumerable<ModuleInfoDto>> GetItemsAsync(bool forceRefresh = false) {
    //        return await Task.FromResult(modules);
    //    }
    //}
}
