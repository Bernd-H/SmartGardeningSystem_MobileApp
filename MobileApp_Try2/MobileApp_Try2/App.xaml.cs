using MobileApp_Try2.Common;
using MobileApp_Try2.Services;
using Spring.Context;
using Spring.Context.Support;
using Xamarin.Forms;

namespace MobileApp_Try2
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
