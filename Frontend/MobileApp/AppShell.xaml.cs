using MobileApp.Common;
using MobileApp.Views;
using Xamarin.Forms;

namespace MobileApp {
    public partial class AppShell : Xamarin.Forms.Shell {
        public AppShell() {
            InitializeComponent();

            Routing.RegisterRoute(PageNames.SGModuleDetailPage, typeof(SGModuleDetailPage));
            Routing.RegisterRoute(PageNames.AccountPage, typeof(AccountPage));
            Routing.RegisterRoute(PageNames.MainPage, typeof(MainPage));
            Routing.RegisterRoute(PageNames.AddModulePage, typeof(AddModulePage));
            Routing.RegisterRoute(PageNames.HelpPage, typeof(HelpPage));
            Routing.RegisterRoute(PageNames.LoginPage, typeof(LoginPage));
            Routing.RegisterRoute(PageNames.ConnectingPage, typeof(ConnectingPage));
            Routing.RegisterRoute(PageNames.LogsPage, typeof(LogsPage));
            Routing.RegisterRoute(PageNames.WaitingForNewModulePage, typeof(WaitingForNewModulePage));
            Routing.RegisterRoute(PageNames.SelectValvePage, typeof(SelectValvePage));
            Routing.RegisterRoute(PageNames.SelectWlanPage, typeof(SelectWlanPage));
            Routing.RegisterRoute(PageNames.ConnectToWlanPage, typeof(ConnectToWlanPage));
            Routing.RegisterRoute(PageNames.SignUpPage, typeof(SignUpPage));
            Routing.RegisterRoute(PageNames.SettingsPage, typeof(SettingsPage));
        }
    }
}
