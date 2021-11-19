using Microcharts;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;

namespace MobileApp.Views {
    public partial class MainPage : ContentPage {

        private MainPageViewModel _viewModel;

        private bool connectingPageSelected = true;

        public MainPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            Shell.SetNavBarIsVisible(this, false);
            BindingContext = _viewModel = IoC.Get<MainPageViewModel>();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        //protected override void OnCurrentPageChanged() {
        //    if (_viewModel != null) {
        //        connectingPageSelected = !connectingPageSelected;

        //        if (!connectingPageSelected) {
        //            // load logs...
        //            _ = _viewModel.LoadLogs();
        //        }
        //    }
        //}
    }
}
