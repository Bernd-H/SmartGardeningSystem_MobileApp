using MobileApp_Try2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileApp_Try2.Services
{
    public class ModulesMockDataStore : IDataStore<SGModule>
    {
        readonly List<SGModule> items;

        public ModulesMockDataStore()
        {
            items = new List<SGModule>()
            {
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Vorgarten sensor links.",
                    MeasuredValue = "40%",
                    Name = "SensVorgartenL"
                },
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Hintergarten sensor links.",
                    MeasuredValue = "38.2%",
                    Name = "SensHintergartenL"
                },
                new SGModule {
                    Id = Guid.NewGuid().ToString(),
                    IsOnline = true,
                    InfoText = "Vorgarten sensor rechts.",
                    MeasuredValue = "52.3%",
                    Name = "SensVorgartenR"
                }
            };
        }

        public async Task<bool> AddItemAsync(SGModule item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(SGModule item)
        {
            var oldItem = items.Where((SGModule arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((SGModule arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<SGModule> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<SGModule>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}
