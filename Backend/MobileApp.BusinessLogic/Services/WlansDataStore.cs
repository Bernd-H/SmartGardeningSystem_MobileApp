using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;

namespace MobileApp.BusinessLogic.Services {
    public class WlansDataStore : IDataStore<WlanInfoDto> {

        private List<WlanInfoDto> wlans;


        private ILogger Logger;

        private IAPIManager APIManager;

        public WlansDataStore(ILoggerService loggerService, IAPIManager apiManager) {
            Logger = loggerService.GetLogger<WlansDataStore>();
            APIManager = apiManager;

            wlans = new List<WlanInfoDto>();
        }

        public async Task<IEnumerable<WlanInfoDto>> GetItemsAsync(bool forceRefresh = false) {
            if (forceRefresh || wlans.Count == 0) {
                var _wlans = await APIManager.GetWlans();
                wlans.Clear();
                if (_wlans != null) {
                    foreach (var wlan in _wlans) {
                        wlans.Add(new WlanInfoDto() {
                            Ssid = wlan.Ssid
                        });
                    }
                }
            }

            return wlans;
        }

        #region not used IDataStore methodes

        public Task<bool> AddItemAsync(WlanInfoDto item) {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteItemAsync(string id) {
            throw new NotImplementedException();
        }

        public Task<WlanInfoDto> GetItemAsync(string id) {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateItemAsync(WlanInfoDto item) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
