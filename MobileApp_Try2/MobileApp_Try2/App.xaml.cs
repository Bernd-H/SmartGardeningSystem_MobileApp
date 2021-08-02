using MobileApp_Try2.Services;
using MobileApp_Try2.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp_Try2
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
            //MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
            Shell.Current.GoToAsync("//LoginPage");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
