using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp_Try2.Common {
    public static class PageNames {
        public static readonly string LoginPage = "LoginPage";

        public static readonly string MainPage = "MainPage";

        public static readonly string SGModuleDetailPage = "SGModuleDetailPage";

        public static readonly string AccountPage = "AccountPage";

        public static readonly string AddModulePage = "AddModulePage";

        public static readonly string HelpPage = "HelpPage";

        public static string GetNavigationString(string page) {
            return $"//{page}";
        }
    }
}
