using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
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
        }

        public void Dispose() {
            client.Dispose();
        }

        public async Task<bool> Login(string email, string password) {
            //await SettingsManager.UpdateCurrentSettings(s => {
            //    s.BaseStationIP = "10.0.2.2";
            //    return s;
            //});

            var settings = await SettingsManager.GetApplicationSettings();

            if (settings.AesIV != null && settings.AesKey != null) {
                // build url
                var config = ConfigurationStore.GetConfig();
                string url = string.Format(config.ConnectionSettings.API_URL_Login, settings.BaseStationIP, config.ConnectionSettings.API_Port);

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

                    return true;
                }
            }


            return false;
        }
    }
}
