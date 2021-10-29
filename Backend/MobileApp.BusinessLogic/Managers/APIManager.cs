using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                } catch(Exception ex) {
                    Logger.Error(ex, $"[Login]Error while logging in. (api-request-url={url})");
                }
            }

            return false;
        }

        public async void Logout() {
            // remove json web token from header
            if (client?.DefaultRequestHeaders.Contains("Authorization") ?? false) {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
        }

        public async Task<IEnumerable<ModuleInfoDto>> GetModules() {
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = string.Format(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    var response = await client.GetAsync(url);
                    string result = await response.Content.ReadAsStringAsync();

                    var modules = JsonConvert.DeserializeObject<List<ModuleInfo>>(result);

                    return modules.ToDtos();
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[GetModules]Could not get modules from rest api. (url={url})");
            }

            return null;
        }


        private async Task<bool> TryAddWebTokenToHeader() {
            var settings = await SettingsManager.GetApplicationSettings();
            if (settings.SessionAPIToken != null) {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.SessionAPIToken.Token}");
                return true;
            }

            return false;
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
                var moduleDto = updatedModule.ToDto();
                string json = JsonConvert.SerializeObject(moduleDto);

                // setup the body of the request
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, data);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                }
                else {
                    Logger.Error($"[UpdateModule]API returned code: {response.StatusCode.ToString()}.");
                }
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
                var moduleDto = newModule.ToDto();
                string json = JsonConvert.SerializeObject(moduleDto);

                // setup the body of the request
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                } else {
                    Logger.Error($"[AddModule]API returned code: {response.StatusCode.ToString()}.");
                }
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

                var response = await client.DeleteAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                    return true;
                }
                else {
                    Logger.Error($"[DeleteModule]API returned code: {response.StatusCode.ToString()}.");
                }
            } catch (Exception ex) {
                Logger.Error(ex, $"[DeleteModule]Could not get modules from rest api. (url={url})");
            }

            return false;
        }
    }
}
