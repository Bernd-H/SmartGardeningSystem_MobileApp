﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileApp_Try2.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp_Try2.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage {
        public AccountPage() {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            this.BindingContext = new AccountViewModel();
        }
    }
}
