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
