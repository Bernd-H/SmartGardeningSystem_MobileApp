using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectValvePage : ContentPage {
        public SelectValvePage() {
            InitializeComponent();

            this.BindingContext = IoC.Get<SelectValvePageViewModel>();
        }
    }
}
