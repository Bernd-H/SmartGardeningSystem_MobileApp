using MobileApp_Try2.Services;
using MobileApp_Try2.Views;
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

            //Routing.RegisterRoute(nameof(SGModuleDetailPage), typeof(SGModuleDetailPage));
            //Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
            //Routing.RegisterRoute(nameof(MobileApp_Try2.Views.MainPage), typeof(MainPage));
            //Routing.RegisterRoute(nameof(AddModulePage), typeof(AddModulePage));

            //MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
            //if (false)
            {
                Shell.Current.GoToAsync("//LoginPage");
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
