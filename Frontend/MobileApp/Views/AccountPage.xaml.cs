using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage {
        public AccountPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            //this.BindingContext = new AccountViewModel();
            this.BindingContext = IoC.Get<AccountViewModel>();
        }
    }
}
