using System.Threading.Tasks;
using MobileApp.BusinessLogic.ViewModels;
using MobileApp.Common.Configuration;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogsPage : ContentPage {
        public LogsPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = IoC.Get<LogsPageViewModel>();
            (BindingContext as LogsPageViewModel).PropertyChanged += LogsPage_PropertyChanged;
        }

        private async void LogsPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Logs") {
                // give the mobile phone time to update the logsLabel
                await Task.Delay(300);

                // scroll to the end
                await scrollView.ScrollToAsync(logsLabel, ScrollToPosition.End, false);
            }
        }
    }
}
