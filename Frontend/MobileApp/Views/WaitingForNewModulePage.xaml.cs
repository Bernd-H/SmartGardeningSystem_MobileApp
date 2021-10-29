using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WaitingForNewModulePage : ContentPage {
        public WaitingForNewModulePage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = IoC.Get<WaitingForNewModulePageViewModel>();
        }
    }
}
