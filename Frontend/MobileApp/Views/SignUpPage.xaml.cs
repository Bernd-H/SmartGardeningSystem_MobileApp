using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUpPage : ContentPage {
        public SignUpPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            Shell.SetNavBarIsVisible(this, false);

            this.BindingContext = IoC.Get<SignUpViewModel>();
        }
    }
}
