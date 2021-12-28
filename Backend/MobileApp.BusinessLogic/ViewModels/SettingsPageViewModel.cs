﻿using System;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class SettingsPageViewModel : BaseViewModel {
        public ICommand ManualControlCommand { get; set; }

        public ICommand AccountSettingsCommand { get; set; }

        public ICommand ChangeWlanCommand { get; set; }

        public ICommand BackCommand { get; set; }



        private ILogger Logger;

        public SettingsPageViewModel(ILoggerService loggerService) {
            Logger = loggerService.GetLogger<SettingsPageViewModel>();

            ManualControlCommand = new Command(ManualControlTapped);
            AccountSettingsCommand = new Command(AccountSettingsTapped);
            ChangeWlanCommand = new Command(ChangeWlanTapped);
            BackCommand = new Command(OnBackTapped);
        }

        private async void OnBackTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
        }

        private async void ChangeWlanTapped(object obj) {
            await Shell.Current.GoToAsync($"{PageNames.SelectWlanPage}?{nameof(SelectWlanPageViewModel.NavigationString)}={PageNames.SettingsPage}");
        }

        private async void AccountSettingsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.AccountPage);
        }

        private async void ManualControlTapped(object obj) {
            //await Shell.Current.GoToAsync(PageNames.)
        }
    }
}
