using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
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

            client = new HttpClient();
        }

        public void Dispose() {
            client.Dispose();
        }

        public async Task<bool> Login(string email, string password) {
            var settings = SettingsManager.GetApplicationSettings();

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

                Logger.Debug(result);
            }


            return false;
        }
    }
}
