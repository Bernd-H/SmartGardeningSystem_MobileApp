using MobileApp.BusinessLogic.Services;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common;
using MobileApp.Common.Specifications.Services;
using MobileApp.Services;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();

            //MainPage = IocGet<AppShell>();
        }

        protected override void OnStart()
        {
            //if (false)
            {
                Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void RegisterDependencies() {
            //DependencyService.Register<ModulesMockDataStore>();
            //DependencyService.Register<IDialogService, DialogService>();

            //DependencyService.Register<MainPageViewModel>();
        }
    }
}
