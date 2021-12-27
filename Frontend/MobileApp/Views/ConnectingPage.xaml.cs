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
        }

        protected override void OnCurrentPageChanged() {
            base.OnCurrentPageChanged();

            if (BindingContext != null) {
                connectingPageSelected = !connectingPageSelected;

                if (!connectingPageSelected) {
                    // load logs...
                   ((ConnectingPageViewModel)BindingContext).LoadLogs(null, null);
                }
            }
        }
    }
}
