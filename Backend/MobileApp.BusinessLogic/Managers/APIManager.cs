using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GardeningSystem.Common.Models.DTOs;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.Managers;
using Newtonsoft.Json;
using NLog;

namespace MobileApp.BusinessLogic.Managers {
    public class APIManager : IAPIManager, IDisposable {

        private readonly HttpClient client;

        private ISettingsManager SettingsManager;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ILogger Logger;

        public APIManager(ILoggerService loggerService, ISettingsManager settingsManager, IAesEncrypterDecrypter aesEncrypterDecrypter) {
            Logger = loggerService.GetLogger<APIManager>();
            SettingsManager = settingsManager;
            AesEncrypterDecrypter = aesEncrypterDecrypter;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new HttpClient(httpClientHandler);

            // add json web token to header if a token exists
            TryAddWebTokenToHeader().Wait();
        }

        ~APIManager() {
            Dispose();
        }

        public void Dispose() {
            client.Dispose();
        }

        public async Task<bool> Login(string email, string password) {
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            if (settings.AesIV != null && settings.AesKey != null) {
                try {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = string.Format(config.ConnectionSettings.API_URL_Login, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    // prepare data to send
                    var userData = new UserDto() {
                        Id = settings.Id,
                        AesEncryptedEmail = AesEncrypterDecrypter.Encrypt(email),
                        AesEncryptedPassword = AesEncrypterDecrypter.Encrypt(password)
                    };
                    string json = JsonConvert.SerializeObject(userData);

                    // setup the body of the request
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, data);
                    string result = await response.Content.ReadAsStringAsync();

                    if (result.Contains("token")) {
                        var jwt = JsonConvert.DeserializeObject<Jwt>(result);
                        jwt.CreationDate = DateTime.UtcNow;

                        // store json web token in settings
                        await SettingsManager.UpdateCurrentSettings(currentSettings => {
                            currentSettings.SessionAPIToken = jwt;
                            return currentSettings;
                        });

                        // add web token to request header
                        return await TryAddWebTokenToHeader();
                    }
                }
                catch (CryptographicException) {
                    throw;
                }
                catch(Exception ex) {
                    Logger.Error(ex, $"[Login]Error while logging in. (api-request-url={url})");
                }
            }

            return false;
        }

        public void Logout() {
            // remove json web token from header
            if (client?.DefaultRequestHeaders.Contains("Authorization") ?? false) {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
        }

        #region Module requests

        public async Task<IEnumerable<ModuleInfoDto>> GetModules() {
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = string.Format(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    var response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }

                    string result = await response.Content.ReadAsStringAsync();

                    var modules = JsonConvert.DeserializeObject<List<ModuleInfo>>(result);

                    return modules.ToDtos();
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[GetModules]Could not get modules from rest api. (url={url})");
            }

            return null;
        }

        public async Task<bool> UpdateModule(ModuleInfo updatedModule) {
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = string.Format(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);
                url += $"{updatedModule.Id.ToString()}"; // add id to url

                // prepare data to send
                //var moduleDto = updatedModule.ToDto();
                string json = JsonConvert.SerializeObject(updatedModule);

                // setup the body of the request
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, data);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    throw new UnauthorizedAccessException();
                }
                else {
                    Logger.Error($"[UpdateModule]API returned code: {response.StatusCode.ToString()}.");
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[UpdateModule]Could not get modules from rest api. (url={url})");
            }

            return false;
        }

        public async Task<bool> AddModule(ModuleInfo newModule) {
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = string.Format(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                // prepare data to send
                //var moduleDto = newModule.ToDto();
                string json = JsonConvert.SerializeObject(newModule);

                // setup the body of the request
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    throw new UnauthorizedAccessException();
                }
                else {
                    Logger.Error($"[AddModule]API returned code: {response.StatusCode.ToString()}.");
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[AddModule]Could not get modules from rest api. (url={url})");
            }

            return false;
        }

        public async Task<bool> DeleteModule(Guid moduleId) {
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = string.Format(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);
                url += $"{moduleId}";

                var response = await client.DeleteAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    // delete it locally if it does not exist on server
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                    throw new UnauthorizedAccessException();
                }
                else {
                    Logger.Error($"[DeleteModule]API returned code: {response.StatusCode.ToString()}.");
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[DeleteModule]Could not get modules from rest api. (url={url})");
            }

            return false;
        }

        #endregion

        public async Task<IEnumerable<WlanInfo>> GetWlans() {
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = string.Format(config.ConnectionSettings.API_URL_Wlan, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    var response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    string result = await response.Content.ReadAsStringAsync();

                    var wlans = JsonConvert.DeserializeObject<List<WlanInfo>>(result);
                    return wlans;
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[GetWlans]Could not get wlans from rest api. (url={url})");
            }

            return null;
        }

        public async Task<bool> IsBasestationConnectedToWlan() {
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = string.Format(config.ConnectionSettings.API_URL_Wlan, settings.BaseStationIP, config.ConnectionSettings.API_Port);
                    url += "isConnected";

                    var response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    string result = await response.Content.ReadAsStringAsync();

                    var isConnectedToWlan = JsonConvert.DeserializeObject<IsConnectedToWlanDto>(result);
                    return isConnectedToWlan.IsConnected;
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[GetModules]Could not get modules from rest api. (url={url})");
            }

            // return false because when the api is not reachable, the user should not get asked what wlan to connect to
            return true;
        }

        private async Task<bool> TryAddWebTokenToHeader() {
            var settings = await SettingsManager.GetApplicationSettings();
            if (settings.SessionAPIToken != null) {
                // remove existing authorization header
                if (client.DefaultRequestHeaders.Contains("Authorization")) {
                    client.DefaultRequestHeaders.Remove("Authorization");
                }

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.SessionAPIToken.Token}");
                return true;
            }

            return false;
        }
    }
}
