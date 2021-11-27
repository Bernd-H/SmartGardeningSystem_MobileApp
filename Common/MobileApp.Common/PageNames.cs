namespace MobileApp.Common {
    public static class PageNames {
        public static readonly string LoginPage = "LoginPage";

        public static readonly string MainPage = "MainPage";

        public static readonly string SGModuleDetailPage = "SGModuleDetailPage";

        public static readonly string AccountPage = "AccountPage";

        public static readonly string AddModulePage = "AddModulePage";

        public static readonly string HelpPage = "HelpPage";

        public static readonly string ConnectingPage = "ConnectingPage";

        public static readonly string LogsPage = "LogsPage";

        public static readonly string WaitingForNewModulePage = "WaitingForNewModulePage";

        public static readonly string SelectValvePage = "SelectValvePage";

        public static readonly string SelectWlanPage = "SelectWlanPage";

        public static readonly string ConnectToWlanPage = "ConnectToWlanPage";

        public static readonly string SignUpPage = "SignUpPage";

        public static string GetNavigationString(string page) {
            return $"//{page}";
        }
    }
}
