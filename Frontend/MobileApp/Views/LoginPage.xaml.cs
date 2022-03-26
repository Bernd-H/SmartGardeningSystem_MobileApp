using System;
using System.Threading.Tasks;
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
            ((LoginViewModel)BindingContext).PropertyChanged += LoginPage_PropertyChanged;
        }

        protected override async void OnCurrentPageChanged() {
            base.OnCurrentPageChanged();

            if (BindingContext != null) {
                loginPageSelected = !loginPageSelected;

                if (!loginPageSelected) {
                    // load logs...
                    await ((LoginViewModel)BindingContext).LoadLogs(null, null);

                    // scroll to the end
                    await scrollView.ScrollToAsync(logsLabel, ScrollToPosition.End, false);
                }
            }
        }

        private async void LoginPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Logs" && !loginPageSelected) {
                // give the mobile phone time to update the logsLabel
                await Task.Delay(300);

                // scroll to the end
                await scrollView.ScrollToAsync(logsLabel, ScrollToPosition.End, false);
            }
        }
    }
}
