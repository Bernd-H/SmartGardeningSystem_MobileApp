using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectWlanPage : ContentPage {

        private SelectWlanPageViewModel viewModel;

        public SelectWlanPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = viewModel = IoC.Get<SelectWlanPageViewModel>();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            viewModel.OnAppearing();
        }
    }
}
