using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectValvePage : ContentPage {

        private SelectValvePageViewModel viewModel;

        public SelectValvePage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = viewModel = IoC.Get<SelectValvePageViewModel>();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            viewModel.OnAppearing();
        }
    }
}
