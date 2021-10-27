using System.Reflection;
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


        private ISettingsManager SettingsManager;

        private IDialogService DialogService;

        private ILogger Logger;

        public AccountViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IDialogService dialogService)
        {
            Logger = loggerService.GetLogger<AccountViewModel>();
            SettingsManager = settingsManager;
            DialogService = dialogService;

            LoggoutCommand = new Command(OnLoggoutClicked);
            ChangePasswordCommand = new Command(OnChangePassword);
            ChangeEmailCommand = new Command(OnChangeEmail);
        }

        private async void OnLoggoutClicked(object obj)
        {
            // delete login information
            Logger.Info($"[OnLoggoutClicked]Deleting json web token.");
            await SettingsManager.UpdateCurrentSettings(currentSettings => {
                currentSettings.SessionAPIToken = null;
                return currentSettings;
            });

            await Shell.Current.GoToAsync($"//{PageNames.LoginPage}");
        }

        private async void OnChangePassword(object obj) {
            await DialogService.ShowMessage("You have been alerted", "Alert", "OK", null);
        }
        private async void OnChangeEmail(object obj) {
            await DialogService.ShowMessage("You have been alerted", "Alert", "OK", null);
        }
    }
}
