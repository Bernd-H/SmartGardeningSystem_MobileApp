using MobileApp_Try2.Views;
using Xamarin.Forms;

namespace MobileApp_Try2
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SGModuleDetailPage), typeof(SGModuleDetailPage));
            Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddModulePage), typeof(AddModulePage));
        }

    }
}
