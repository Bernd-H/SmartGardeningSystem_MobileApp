using System;
using System.Reflection;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public Command LoggoutCommand { get; }
        public Command ChangePasswordCommand { get; }
        public Command ChangeEmailCommand { get; }

        public ICommand BackCommand { get; }


        private ISettingsManager SettingsManager;

        private IDialogService DialogService;

        private IAPIManager APIManager;

        private ILogger Logger;

        public AccountViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IDialogService dialogService, IAPIManager _APIManager)
        {
            Logger = loggerService.GetLogger<AccountViewModel>();
            SettingsManager = settingsManager;
            DialogService = dialogService;
            APIManager = _APIManager;

            LoggoutCommand = new Command(OnLoggoutClicked);
            ChangePasswordCommand = new Command(OnChangePassword);
            ChangeEmailCommand = new Command(OnChangeEmail);
            BackCommand = new Command(OnBackCommand);
        }

        private async void OnBackCommand(object obj) {
            //await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
            await Shell.Current.GoToAsync(PageNames.SettingsPage);
        }

        private async void OnLoggoutClicked(object obj)
        {
            // delete login information
            Logger.Info($"[OnLoggoutClicked]Deleting json web token.");
            await SettingsManager.UpdateCurrentSettings(currentSettings => {
                currentSettings.SessionAPIToken = null;
                return currentSettings;
            });

            APIManager.Logout();

            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
        }

        private async void OnChangePassword(object obj) {
            await DialogService.ShowMessage("You have been alerted", "Alert", "OK", null);
        }
        private async void OnChangeEmail(object obj) {
            await DialogService.ShowMessage("You have been alerted", "Alert", "OK", null);
        }
    }
}
