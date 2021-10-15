using MobileApp.BusinessLogic;
using MobileApp.BusinessLogic.Managers;
using MobileApp.BusinessLogic.Services;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common;
using MobileApp.Common.Models;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Services;
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
            //if (false)
            {
                Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
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
            var container = TinyIoCContainer.Current;

            // services
            container.Register<IDialogService, DialogService>();
            container.Register<ILoggerService, LoggerService>();

            // managers
            container.Register<IAPIManager, APIManager>().AsSingleton();
            container.Register<ISettingsManager, SettingsManager>().AsSingleton();
            container.Register<IConfigurationManager, ConfigurationManager>().AsSingleton();

            // other
            // warning: asSingleton only needed by ModulesMockDataStore, because new fake ids would get created every time it gets created.
            container.Register<IDataStore<SGModule>, ModulesMockDataStore>().AsSingleton();

            // register view models
            container.Register<AccountViewModel>();
            container.Register<AddModuleViewModel>();
            container.Register<LoginViewModel>();
            container.Register<MainPageViewModel>();
            container.Register<SGModuleDetailViewModel>();
        }
    }
}
