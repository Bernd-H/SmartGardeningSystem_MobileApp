using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Managers;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Utilities;
using MobileApp.DataAccess;
using MobileApp.DI;
using NLog;
using TinyIoC;

namespace ExternalServerTest
{
    /// <summary>
    /// Connects many clients to the RelayInitManager at the same time.
    /// </summary>
    class Program
    {
        static void Main(string[] args) {
            DI.RegisterDependencies();
            LoggerService.AddCustomLogTargets();
            RegisterPlatformSpecificDependencies();
            SettingsManager.PLATFORM_IS_WINDOWS = true;
            TinyIoCContainer.Current.Register<RelayInitConnectionTest>().AsSingleton();

            // load config file
            ConfigurationStore.ConfigurationContent = File.ReadAllText(ConfigurationStore.ConfigFileName);

            var logger = IoC.Get<ILoggerService>().GetLogger<Program>();
            logger.Info($"[Main]Starting up test program...");

            try {
                var test = IoC.Get<RelayInitConnectionTest>();

                var testTask = test.Start(amountOfConnections: 20);

                Console.WriteLine("Press enter to stop the test.");
                Console.ReadLine();

                test.Stop();

                testTask.Wait();
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

     internal class RelayInitConnectionTest {

        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ConcurrentBag<Stream> _connections = new ConcurrentBag<Stream>();

        private ILogger Logger;

        public RelayInitConnectionTest(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<RelayInitConnectionTest>();
        }

        /// <summary>
        /// Opens <paramref name="amountOfConnections"/> concurrent connections to the relay init manager and leaves them open.
        /// </summary>
        /// <param name="amountOfConnections">Amount of concurrent connections.</param>
        /// <returns>A Task on which the Test runs.</returns>
        public Task Start(int amountOfConnections = 10) {
            return Task.Run(() => {
                var config = ConfigurationStore.GetConfig();
                string targetHost = config.ConnectionSettings.ExternalServer_Domain;
                var ip = IpUtils.GetHostAddress(config.ConnectionSettings.ExternalServer_Domain, 5000);

                var plr = Parallel.For(0, amountOfConnections, i => {
                    var client = IoC.Get<ISslTcpClient>();

                    bool connectedSuccessfully = client.Start(new IPEndPoint(ip, Convert.ToInt32(config.ConnectionSettings.ExternalServer_RelayPort)), selfSignedCertificate: false, targetHost).Result;

                    if (connectedSuccessfully) {
                        // add the open ssl stream to a concurrent list
                        _connections.Add(client.SslStream);
                    }

                    client.Stop();
                    Logger.Info($"[Start]Connection {i} connected: {connectedSuccessfully}.");
                });
            }, _cts.Token);
        }

        /// <summary>
        /// Stops the Test Task and closes all opened connections.
        /// </summary>
        public void Stop() {
            // stops the process that makes new connections
            _cts.Cancel();
            // close all open connections
            closeAllConnections();
        }

        private void closeAllConnections() {
            Logger.Info($"[closeAllConnections]Closing {_connections.Count} open connections.");
            foreach (var conn in _connections) {
                conn?.Close();
            }
        }
    }
}
