using MobileApp.Common;
using MobileApp.Services;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            DependencyService.Register<ModulesMockDataStore>();
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
    }
}
