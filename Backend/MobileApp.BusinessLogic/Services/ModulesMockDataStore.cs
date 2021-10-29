using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.BusinessLogic.Services {
    public class ModulesMockDataStore : IDataStore<ModuleInfoDto> {
        readonly List<ModuleInfoDto> items;

        public ModulesMockDataStore() {
            items = new List<ModuleInfoDto>()
            {
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.MAINSTATION),
                    Name = "Hauptstation"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.SENSOR),
                    Name = "Sensor1"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.SENSOR),
                    Name = "Sensor2"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.VALVE),
                    Name = "Valve1"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.VALVE),
                    Name = "Valve2"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.VALVE),
                    Name = "Valve3"
                },
                new ModuleInfoDto {
                    Id = Guid.NewGuid(),
                    Type = new ModuleTypes(ModuleTypes.VALVE),
                    Name = "Valve4"
                }
            };
        }

        public async Task<bool> AddItemAsync(ModuleInfoDto item) {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ModuleInfoDto item) {
            var oldItem = items.Where((ModuleInfoDto arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id) {
            var oldItem = items.Where((ModuleInfoDto arg) => arg.Id.ToString() == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<ModuleInfoDto> GetItemAsync(string id) {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id.ToString() == id));
        }

        public async Task<IEnumerable<ModuleInfoDto>> GetItemsAsync(bool forceRefresh = false) {
            return await Task.FromResult(items);
        }
    }
}
