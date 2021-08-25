using System;
using MobileApp_Try2.BusinessLogic.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp_Try2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            Shell.SetNavBarIsVisible(this, false);
            this.BindingContext = new LoginViewModel();
        }
    }
}
