using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Cryptography;
using MobileApp.BusinessLogic.Managers;
using MobileApp.BusinessLogic.Services;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common;
using MobileApp.Common.Configuration;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.DataAccess.Communication;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.DataAccess.Communication;
using TinyIoC;
using Xamarin.Forms;

namespace MobileApp {
    public partial class App : Application {

        public App() {
            InitializeComponent();
            RegisterDependencies();

            MainPage = new AppShell();
        }

        protected override void OnStart() {
            //IoC.Get<ISettingsManager>().UpdateCurrentSettings(cs => {
            //    return ApplicationSettingsDto.GetStandardSettings();
            //}).Wait();

            var settings = IoC.Get<ISettingsManager>().GetApplicationSettings().Result;

            // check if basestation ip is known and available
            bool isAvailable = false;
            if (!string.IsNullOrEmpty(settings.BaseStationIP)) {
                isAvailable = BasestationFinderManager.IsHostAvailable(settings.BaseStationIP);
            }

            if (settings.AesKey != null && settings.AesIV != null && isAvailable) {
                if (settings.SessionAPIToken == null) {
                    // login credentials will get encrypted with the aes server key after "login" gets pressed
                    Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
                }
                else {
                    // already logged in
                    // go to main page
                    Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
                }
            }
            else {
                // to get ip and aes key from basestation (server)
                Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.ConnectingPage));
            }
        }

        protected override void OnSleep() {
        }

        protected override void OnResume() {
        }

        /// <summary>
        /// Registers only dependencies, which are not platform specific.
        /// Platform specific dependencies get registered in the particular start project (MobileApp.Android, MobileApp.iOS).
        /// </summary>
        private void RegisterDependencies() {
            // Info: By default TinyIoC will register concrete classes as multi-instance and interface registrations as singletons.
            var container = TinyIoCContainer.Current;

            // services
            container.Register<IDialogService, DialogService>();
            container.Register<ILoggerService, LoggerService>();

            // managers
            container.Register<IAPIManager, APIManager>();
            container.Register<ISettingsManager, SettingsManager>();
            container.Register<IAesKeyExchangeManager, AesKeyExchangeManager>();
            container.Register<IBasestationFinderManager, BasestationFinderManager>();

            // communication
            container.Register<ISslTcpClient, SslTcpClient>();
            container.Register<ILocalBasestationDiscovery, LocalBasestationDiscovery>();
            container.Register<IMulticastUdpSender, MulticastUdpSender>();

            // other
            // warning: asSingleton only needed by ModulesMockDataStore, because new fake ids would get created every time it gets created.
            //container.Register<IDataStore<SGModule>, ModulesMockDataStore>();
            container.Register<IDataStore<ModuleInfoDto>, ModuleDataStore>();

            container.Register<IAesEncrypterDecrypter, AesEncrypterDecrypter>();

            // register view models
            container.Register<AccountViewModel>().AsSingleton();
            container.Register<AddModuleViewModel>().AsSingleton();
            container.Register<LoginViewModel>().AsSingleton();
            container.Register<MainPageViewModel>().AsSingleton();
            container.Register<SGModuleDetailViewModel>().AsSingleton();
        }
    }
}
