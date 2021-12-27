using System;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : CarouselPage {

        private bool loginPageSelected = true;

        public LoginPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            Shell.SetNavBarIsVisible(this, false);

            this.BindingContext = IoC.Get<LoginViewModel>();
        }

        protected override void OnCurrentPageChanged() {
            base.OnCurrentPageChanged();

            if (BindingContext != null) {
                loginPageSelected = !loginPageSelected;

                if (!loginPageSelected) {
                    // load logs...
                    ((LoginViewModel)BindingContext).LoadLogs(null, null);
                }
            }
        }
    }
}
