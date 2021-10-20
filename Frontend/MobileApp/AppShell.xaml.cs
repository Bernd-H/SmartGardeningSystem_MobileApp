﻿using MobileApp.Common;
using MobileApp.Views;
using Xamarin.Forms;

namespace MobileApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Routing.RegisterRoute(nameof(SGModuleDetailPage), typeof(SGModuleDetailPage));
            //Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
            //Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            //Routing.RegisterRoute(nameof(AddModulePage), typeof(AddModulePage));
            //Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));

            Routing.RegisterRoute(PageNames.SGModuleDetailPage, typeof(SGModuleDetailPage));
            Routing.RegisterRoute(PageNames.AccountPage, typeof(AccountPage));
            Routing.RegisterRoute(PageNames.MainPage, typeof(MainPage));
            Routing.RegisterRoute(PageNames.AddModulePage, typeof(AddModulePage));
            Routing.RegisterRoute(PageNames.HelpPage, typeof(HelpPage));
            Routing.RegisterRoute(PageNames.LoginPage, typeof(LoginPage));
            Routing.RegisterRoute(PageNames.ConnectingPage, typeof(ConnectingPage));
        }

    }
}
