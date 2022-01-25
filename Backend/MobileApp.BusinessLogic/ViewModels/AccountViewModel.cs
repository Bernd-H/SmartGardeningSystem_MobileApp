using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        private string currentUsername;
        public string CurrentUsername {
            get => currentUsername;
            set => SetProperty(ref currentUsername, value);
        }

        private string newUsername;
        public string NewUsername {
            get => newUsername;
            set => SetProperty(ref newUsername, value);
        }


        private string currentPassword;
        public string CurrentPassword {
            get => currentPassword;
            set => SetProperty(ref currentPassword, value);
        }

        private string newPassword;
        public string NewPassword {
            get => newPassword;
            set => SetProperty(ref newPassword, value); 
        }

        private string confirmNewPassword;
        public string ConfirmNewPassword {
            get => confirmNewPassword;
            set => SetProperty(ref confirmNewPassword, value);
        }

        public ICommand LoggoutCommand { get; }

        public ICommand SaveCommand { get; }

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
            SaveCommand = new Command(OnSave);
            BackCommand = new Command(OnBackCommand);
        }

        private async void OnBackCommand(object obj) {
            CurrentPassword = "";
            NewPassword = "";
            ConfirmNewPassword = "";
            CurrentUsername = "";
            NewUsername = "";
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

        private async void OnSave(object obj) {
            if (NewPassword != ConfirmNewPassword) {
                await DialogService.ShowMessage("Entered Passwords are not the same.", "Error", "OK", null);
                return;
            }
            if (string.IsNullOrWhiteSpace(CurrentUsername) || string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(NewUsername)) {
                await DialogService.ShowMessage("Please fill out all fields proberly.", "Error", "OK", null);
                return;
            }

            // send current and new login information
            bool success = await APIManager.ChangeLoginInfo(new Common.Models.DTOs.UpdateUserDto {
                Username = CurrentUsername,
                Password = CurrentPassword,
                NewUsername = NewUsername,
                NewPassword = NewPassword
            });

            if (success) {
                await DialogService.ShowMessage("Successfully changed the login information.", "Info", "OK", null);
                OnBackCommand(null);
            }
            else {
                await DialogService.ShowMessage("Either the current login information is wrong or there was another exception.", "Error", "OK", null);
            }
        }
    }
}
