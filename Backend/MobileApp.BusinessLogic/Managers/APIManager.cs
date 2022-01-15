using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
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

        private X509Certificate basestationCert;

        private ISettingsManager SettingsManager;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ILogger Logger;

        public APIManager(ILoggerService loggerService, ISettingsManager settingsManager, IAesEncrypterDecrypter aesEncrypterDecrypter) {
            Logger = loggerService.GetLogger<APIManager>();
            SettingsManager = settingsManager;
            AesEncrypterDecrypter = aesEncrypterDecrypter;

            basestationCert = SettingsManager.GetApplicationSettings().Result.BasestationCert;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.None;
            httpClientHandler.ServerCertificateCustomValidationCallback = serverCertificateValidationCallback;
            client = new HttpClient(httpClientHandler);
            client.Timeout = TimeSpan.FromMilliseconds(5000);

            // add json web token to header if a token exists
            tryAddWebTokenToHeader().Wait();
        }

        ~APIManager() {
            Dispose();
        }

        public void Dispose() {
            client.Dispose();
        }

        public async Task<bool> Login(string email, string password) {
            Logger.Info($"[Login]Login process initiated.");
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            if (settings.AesIV != null && settings.AesKey != null) {
                try {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_Login, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    // prepare data to send
                    var userData = new UserDto() {
                        //AesEncryptedEmail = AesEncrypterDecrypter.Encrypt(email),
                        //AesEncryptedPassword = AesEncrypterDecrypter.Encrypt(password)
                        Username = email,
                        Password = password
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
                        return await tryAddWebTokenToHeader();
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
            Logger.Trace($"[Logout]Removing authorization header from http client.");

            // remove json web token from header
            if (client?.DefaultRequestHeaders.Contains("Authorization") ?? false) {
                client.DefaultRequestHeaders.Remove("Authorization");
            }
        }

        public async Task<bool> Register(string email, string password) {
            Logger.Info($"[Register]Sending registration data.");
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            if (settings.AesIV != null && settings.AesKey != null) {
                try {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_Register, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    // prepare data to send
                    var userData = new UserDto() {
                        //Id = Guid.NewGuid(),
                        //AesEncryptedEmail = AesEncrypterDecrypter.Encrypt(email),
                        //AesEncryptedPassword = AesEncrypterDecrypter.Encrypt(password)
                        Username = email,
                        Password = password
                    };
                    string json = JsonConvert.SerializeObject(userData);

                    // setup the body of the request
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, data);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                        return true;
                    }
                }
                catch (CryptographicException) {
                    throw;
                }
                catch (Exception ex) {
                    Logger.Error(ex, $"[Register]Error while user registration. (api-request-url={url})");
                }
            }

            return false;
        }

        #region Module requests

        public async Task<IEnumerable<ModuleInfoDto>> GetModules() {
            Logger.Info($"[GetModules]Requesting all modules.");
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);

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
            Logger.Info($"[UpdateModule]Sending updated module configuration data.");
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = getUrl(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);
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
            Logger.Info($"[AddModule]Sending a new module configuration.");
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = getUrl(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);

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
            Logger.Info($"[DeleteModule]Sending remove module request.");
            var settings = await SettingsManager.GetApplicationSettings();
            string url = "";

            try {
                // build url
                var config = ConfigurationStore.GetConfig();
                url = getUrl(config.ConnectionSettings.API_URL_Modules, settings.BaseStationIP, config.ConnectionSettings.API_Port);
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
            Logger.Info($"[GetWlans]Requesting all reachable wlans.");
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_Wlan, settings.BaseStationIP, config.ConnectionSettings.API_Port);

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

        public async Task<SystemStatus> GetSystemStatus() {
            Logger.Info($"[GetSystemStatus]Requesting the system status.");
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_SystemStatus, settings.BaseStationIP, config.ConnectionSettings.API_Port);

                    var response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                        throw new UnauthorizedAccessException();
                    }
                    string result = await response.Content.ReadAsStringAsync();

                    var systemStatusDto = JsonConvert.DeserializeObject<SystemStatusDto>(result);
                    return systemStatusDto.FromDto();
                }
            }
            catch (UnauthorizedAccessException) {
                throw;
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[GetSystemStatus]Could not get system status from rest api. (url={url})");
            }

            return null;
        }

        public async Task<bool> IsBasestationConnectedToWlan() {
            Logger.Info($"[IsBasestationConnectedToWlan]Requesting information, if the basestation is currently connected to a wlan.");
            var settings = await SettingsManager.GetApplicationSettings();

            string url = "";
            try {
                if (settings.SessionAPIToken != null) {
                    // build url
                    var config = ConfigurationStore.GetConfig();
                    url = getUrl(config.ConnectionSettings.API_URL_Wlan, settings.BaseStationIP, config.ConnectionSettings.API_Port);
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
                Logger.Error(ex, $"[IsBasestationConnectedToWlan]Failed to get the wlan status of the basestation from the rest api. (url={url})");
            }

            // return false because when the api is not reachable, the user should not get asked what wlan to connect to
            return true;
        }

        /// <summary>
        /// Validates the server certificate.
        /// </summary>
        /// <returns>True, when <paramref name="serverCert"/> got verified.</returns>
        private bool serverCertificateValidationCallback(HttpRequestMessage arg1, X509Certificate2 serverCert, X509Chain arg3, SslPolicyErrors arg4) {
            if (basestationCert != null) {
                bool storedCertExpired = DateTime.Parse(basestationCert.GetExpirationDateString()).CompareTo(DateTime.Now) <= 0;
                
                if (storedCertExpired) {
                    Logger.Info($"[serverCertificateValidationCallback]Stored server cert expired. Forbidding connection.");
                }
                // check if basestation is the right one
                else if (serverCert.GetCertHash().SequenceEqual(basestationCert.GetCertHash())) {
                    // certificate verified
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> tryAddWebTokenToHeader() {
            var settings = await SettingsManager.GetApplicationSettings();
            if (settings.SessionAPIToken != null) {
                Logger.Info($"[tryAddWebTokenToHeader]Adding a authorization token to the api-client-header.");

                // remove existing authorization header
                if (client.DefaultRequestHeaders.Contains("Authorization")) {
                    client.DefaultRequestHeaders.Remove("Authorization");
                }

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.SessionAPIToken.Token}");
                return true;
            }

            return false;
        }

        private string getUrl(string url, string ip, int port, params object[] otherArgs) {
            if (SettingsManager.GetRuntimeVariables().RelayModeActive) {
                // modify url to relay all requests to a local server
                ip = "127.0.0.1";
                port = 5000;

                if (url.Contains("https")) {
                    url = url.Replace("https", "http");
                }
            }

            return string.Format(url, ip, port, otherArgs);
        }
    }
}
