using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Managers;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using MobileApp.DataAccess;
using MobileApp.DI;
using NLog;
using TinyIoC;

namespace BasestationStressTest {

    /// <summary>
    /// Sends and receives many concurrent packages over the internet to the basestation.
    /// </summary>
    class Program {
        static void Main(string[] args) {
            DI.RegisterDependencies();
            LoggerService.AddCustomLogTargets();
            RegisterPlatformSpecificDependencies();
            SettingsManager.PLATFORM_IS_WINDOWS = true;
            TinyIoCContainer.Current.Register<RelayTest>().AsSingleton();

            // load config file
            ConfigurationStore.ConfigurationContent = File.ReadAllText(ConfigurationStore.ConfigFileName);

            var logger = IoC.Get<ILoggerService>().GetLogger<Program>();
            logger.Info($"[Main]Starting up test program...");

            try {

                // delete stored aes key
                IoC.Get<ISettingsManager>().UpdateCurrentSettings((currentSettings) => {
                    currentSettings.AesIV = null;
                    currentSettings.AesKey = null;
                    return currentSettings;
                }).Wait();

                var test = IoC.Get<RelayTest>();

                var init = test.ExchangeAllNeccessaryKeys().Result;
                if (init) {
                    RelayManager.TEST_PACKET_LENGTH_KB = 100;
                    var testTask = test.Start(amountOfConcurrentTests: 10, forceRelay: true);

                    Console.WriteLine("Press enter to stop the test.");
                    Console.ReadLine();

                    test.Stop();

                    testTask.Wait();
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

    public class RelayTest {

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ILogger Logger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private ISettingsManager SettingsManager;

        public RelayTest(ILoggerService loggerService, IBasestationFinderManager basestationFinderManager,
            IAesKeyExchangeManager aesKeyExchangeManager, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<RelayTest>();
            BasestationFinderManager = basestationFinderManager;
            AesKeyExchangeManager = aesKeyExchangeManager;
            SettingsManager = settingsManager;
        }

        /// <summary>
        /// Starts establishing <paramref name="amountOfConcurrentTests"/> connections at the same time 
        /// and performs a packet ping-pong test on all connections.
        /// </summary>
        /// <param name="amountOfConcurrentTests">Amount of concurrent connections.</param>
        /// <param name="forceRelay">True when all traffic should get relayed over the external server (Forbidds peer to peer connections).</param>
        /// <returns>A Task on which the Test runs.</returns>
        public Task Start(int amountOfConcurrentTests = 10, bool forceRelay = true) {
            return Task.Run(() => {
                Parallel.For(0, amountOfConcurrentTests, i => {
                    var relayManager = IoC.Get<IRelayManager>();
                    var success = relayManager.ConnectToTheBasestation(_cts.Token, forceRelay: forceRelay, test: true).Result;
                    Logger.Info($"[Start]Test result {i}: {success}.");
                });

                Logger.Info($"[Start]---------- Finished ----------");
            });
        }

        /// <summary>
        /// Exchanges an aes key when there is not already one.
        /// </summary>
        /// <remarks>This aes key is needed to connect to the basestation over the internet.</remarks>
        /// <returns>A Task that reprecents an asynchronous operation. The value of the TResult parameter contains a boolean indicating whether
        /// the operation was a success or not.</returns>
        public async Task<bool> ExchangeAllNeccessaryKeys() {
            bool success = false;
            var settings = await SettingsManager.GetApplicationSettings();

            if (settings.AesKey == null || settings.AesIV == null) {
                // find basestaiton locally and exchange a aes key
                var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();

                if (baseStationFound) {
                    // perform key exchange
                    Logger.Info($"[ExchangeAllNeccessaryKeys]Trying to get a aes key from the basestation.");
                    success = await AesKeyExchangeManager.Start(_cts.Token);
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

        /// <summary>
        /// Stops the test and closes all currently opend connections.
        /// </summary>
        public void Stop() {
            Logger.Info($"[Stop]Closeing all connections...");
            _cts.Cancel();
        }
    }
}
