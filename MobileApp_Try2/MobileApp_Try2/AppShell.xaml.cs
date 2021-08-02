using MobileApp_Try2.ViewModels;
using MobileApp_Try2.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MobileApp_Try2
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
