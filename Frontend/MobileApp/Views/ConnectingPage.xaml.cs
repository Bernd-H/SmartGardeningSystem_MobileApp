using System.Threading.Tasks;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectingPage : CarouselPage {

        private bool connectingPageSelected = true;

        public ConnectingPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = IoC.Get<ConnectingPageViewModel>();

            ((ConnectingPageViewModel)BindingContext).PropertyChanged += ConnectingPage_PropertyChanged;
        }

        protected override async void OnCurrentPageChanged() {
            base.OnCurrentPageChanged();

            if (BindingContext != null) {
                connectingPageSelected = !connectingPageSelected;

                if (!connectingPageSelected) {
                    // load logs...
                   await ((ConnectingPageViewModel)BindingContext).LoadLogs(null, null);

                    // scroll to the end
                    await scrollView.ScrollToAsync(logsLabel, ScrollToPosition.End, false);
                }
            }
        }

        private async void ConnectingPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Logs" && !connectingPageSelected) {
                // give the mobile phone time to update the logsLabel
                await Task.Delay(300);

                // scroll to the end
                await scrollView.ScrollToAsync(logsLabel, ScrollToPosition.End, false);
            }
        }
    }
}
