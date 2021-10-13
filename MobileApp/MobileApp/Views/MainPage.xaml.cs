using Microcharts;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;

namespace MobileApp.Views {
    public partial class MainPage : ContentPage {
        MainPageViewModel _viewModel;

        public MainPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            Shell.SetNavBarIsVisible(this, false);
            //this.BindingContext = new MainPageViewModel();
            //BindingContext = _viewModel = new MainPageViewModel();
            BindingContext = _viewModel = IoC.Get<MainPageViewModel>();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}
