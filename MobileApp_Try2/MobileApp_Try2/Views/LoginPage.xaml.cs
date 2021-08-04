using MobileApp_Try2.ViewModels;
using System;
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
            this.BindingContext = new LoginViewModel();
        }

        void LoginClick(object sender, EventArgs args) // Tapped="LoginClick"
        {
            Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
    }
}