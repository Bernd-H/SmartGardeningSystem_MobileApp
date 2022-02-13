using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Managers;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using MobileApp.DataAccess;
using MobileApp.DI;
using NLog;
using TinyIoC;

namespace BasestationCommandManagerTest
{
    class Program
    {
        static async Task Main(string[] args) {
            DI.RegisterDependencies();
            LoggerService.AddCustomLogTargets();
            RegisterPlatformSpecificDependencies();
            SettingsManager.PLATFORM_IS_WINDOWS = true;
            TinyIoCContainer.Current.Register<Test>().AsSingleton();

            // load config file
            ConfigurationStore.ConfigurationContent = File.ReadAllText(ConfigurationStore.ConfigFileName);

            var logger = IoC.Get<ILoggerService>().GetLogger<Program>();
            logger.Info($"[Main]Starting up the test program...");

            // Test begin ---------------------

            try {
                var test = IoC.Get<Test>();

                var keyExchanged = await test.ExchangeAllNeccessaryKeys();
                logger.Info($"Key exchanaged: {keyExchanged}");

                if (keyExchanged) {
                    logger.Info($"Test result: {await test.StartTest()}");
                }
            }
            catch (Exception ex) {
                logger.Fatal(ex, "[Main]An error occured.");
            }

            logger.Info($"[Main]Finished.");
            Console.ReadLine();
        }

        private static void RegisterPlatformSpecificDependencies() {
            var container = TinyIoCContainer.Current;

            container.Register<IFileStorage, FileStorageWin>().AsMultiInstance();

            //container.Register<ITextWidth, CalculateTextWidthAndroid>().AsSingleton();
            //container.Register<ICloseApplicationService, CloseApplicationService>();
        }
    }

    internal class Test {

        private ILogger Logger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private ISettingsManager SettingsManager;

        private ICommandManager CommandManager;

        public Test(ILoggerService loggerService, IBasestationFinderManager basestationFinderManager,
            IAesKeyExchangeManager aesKeyExchangeManager, ISettingsManager settingsManager, ICommandManager commandManager) {
            Logger = loggerService.GetLogger<Test>();
            BasestationFinderManager = basestationFinderManager;
            AesKeyExchangeManager = aesKeyExchangeManager;
            SettingsManager = settingsManager;
            CommandManager = commandManager;
        }

        public Task<bool> StartTest() {
            return CommandManager.Test();
        }

        /// <summary>
        /// Exchanges an aes key when there is not already one.
        /// </summary>
        /// <remarks>This aes key is needed to connect to the basestation over the internet.</remarks>
        /// <returns>A Task that reprecents an asynchronous operation. The value of the TResult parameter contains a boolean indicating whether
        /// the operation was a success or not.</returns>
        public async Task<bool> ExchangeAllNeccessaryKeys() {
            // delete stored aes key
            IoC.Get<ISettingsManager>().UpdateCurrentSettings((currentSettings) => {
                return ApplicationSettingsDto.GetStandardSettings();
            }).Wait();

            bool success = false;
            var settings = await SettingsManager.GetApplicationSettings();

            if (settings.AesKey == null || settings.AesIV == null) {
                // find basestaiton locally and exchange a aes key
                var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();

                if (baseStationFound) {
                    // perform key exchange
                    Logger.Info($"[ExchangeAllNeccessaryKeys]Trying to get a aes key from the basestation.");
                    success = await AesKeyExchangeManager.Start(CancellationToken.None);
                }
                else {
                    Logger.Error($"[ExchangeAllNeccessaryKeys]No aes key stored and no basestation found.");
                }
            }
            else {
                // key is already exchanged and available
                Logger.Info($"[ExchangeAllNeccessaryKeys]Using the saved aes key for the test.");
                success = true;
            }

            return success;
        }
    }
}
