using System;
using System.IO;
using System.Text;
using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Cryptography;
using MobileApp.BusinessLogic.Managers;
using MobileApp.BusinessLogic.Services;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using MobileApp.DataAccess;
using MobileApp.DataAccess.Communication;
using TinyIoC;

namespace MobileApp.DI
{
    public static class DI
    {
        /// <summary>
        /// Registers only dependencies, which are not platform specific.
        /// Platform specific dependencies get registered in the particular start project (MobileApp.Android, MobileApp.iOS).
        /// </summary>
        public static void RegisterDependencies() {
            // Info: By default TinyIoC will register concrete classes as multi-instance and interface registrations as singletons.
            var container = TinyIoCContainer.Current;

            // services
            container.Register<IDialogService, DialogService>();
            container.Register<ILoggerService, LoggerService>().AsMultiInstance();
            container.Register<ICachePageDataService, CachePageDataService>();

            // managers
            container.Register<IAPIManager, APIManager>().AsMultiInstance();
            container.Register<ISettingsManager, SettingsManager>();
            container.Register<IAesKeyExchangeManager, AesKeyExchangeManager>().AsMultiInstance();
            container.Register<IBasestationFinderManager, BasestationFinderManager>().AsMultiInstance();
            container.Register<ICommandManager, CommandManager>().AsMultiInstance();
            container.Register<IRelayManager, RelayManager>().AsMultiInstance();

            // communication
            container.Register<ISslTcpClient, SslTcpClient>().AsMultiInstance();
            container.Register<ILocalBasestationDiscovery, LocalBasestationDiscovery>().AsMultiInstance();
            container.Register<IMulticastUdpSender, MulticastUdpSender>().AsMultiInstance();
            container.Register<IAesTcpClient, AesTcpClient>().AsMultiInstance();
            container.Register<IApiRequestsRelayServer, ApiRequestsRelayServer>().AsMultiInstance();
            container.Register<ICommandsRelayServer, CommandsRelayServer>().AsMultiInstance();

            // other
            // warning: asSingleton only needed by ModulesMockDataStore, because new fake ids would get created every time it gets created.
            //container.Register<IDataStore<ModuleInfoDto>, ModulesMockDataStore>();
            container.Register<IDataStore<ModuleInfo>, ModuleDataStore>();
            container.Register<IDataStore<WlanInfoDto>, WlansDataStore>();

            container.Register<ISecureStorage, SecureDataStorage>().AsMultiInstance();

            container.Register<IAesEncrypterDecrypter, AesEncrypterDecrypter>().AsMultiInstance();
            container.Register<IAesHandlerWithoutSettings, AesHandlerWithoutSettings>().AsMultiInstance();
            container.Register<IAesTunnelInSslStream, AesTunnelInSslStream>().AsMultiInstance();

            // register view models
            container.Register<AccountViewModel>().AsSingleton();
            container.Register<AddModuleViewModel>().AsMultiInstance();
            container.Register<LoginViewModel>().AsMultiInstance();
            container.Register<MainPageViewModel>().AsSingleton();
            container.Register<SGModuleDetailViewModel>().AsMultiInstance();
            container.Register<WaitingForNewModulePageViewModel>().AsSingleton();
            container.Register<SelectValvePageViewModel>().AsMultiInstance();
            container.Register<SelectWlanPageViewModel>().AsMultiInstance();
            container.Register<ConnectToWlanPageViewModel>().AsMultiInstance();
            container.Register<SettingsPageViewModel>().AsSingleton();
        }
    }
}
