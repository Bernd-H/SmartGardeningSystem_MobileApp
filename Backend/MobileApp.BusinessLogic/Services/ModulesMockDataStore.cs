using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileApp.Common.Models;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.Services {
    public class ModulesMockDataStore : IDataStore<SGModule> {
        readonly List<SGModule> items;

        public ModulesMockDataStore() {
            items = new List<SGModule>()
            {
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Für mehr Infos drücken...",
                    _Type = SGModuleType.MAINSTATION,
                    MeasuredValue = "-",
                    Name = "Hauptstation"
                },
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Für mehr Infos drücken...",
                    _Type = SGModuleType.SENSOR,
                    MeasuredValue = "40%",
                    Name = "Vorgarten Links"
                },
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Für mehr Infos drücken...",
                    MeasuredValue = "closed",
                    _Type = SGModuleType.ACTOR,
                    Name = "Vorgarten Links"
                },
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Für mehr Infos drücken...",
                    MeasuredValue = "open",
                    Name = "Ventil 2",
                    _Type = SGModuleType.ACTOR
                }
            };
        }

        public async Task<bool> AddItemAsync(SGModule item) {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(SGModule item) {
            var oldItem = items.Where((SGModule arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id) {
            var oldItem = items.Where((SGModule arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<SGModule> GetItemAsync(string id) {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<SGModule>> GetItemsAsync(bool forceRefresh = false) {
            return await Task.FromResult(items);
        }
    }
}
