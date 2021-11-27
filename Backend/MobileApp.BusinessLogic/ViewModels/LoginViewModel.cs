using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public Command SignUpCommand { get; }


        private string email;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password;
        public string Password {
            get => password;
            set => SetProperty(ref password, value);
        }

        #region Snack bar elements

        private string snackBarMessage;
        public string SnackBarMessage {
            get => snackBarMessage;
            set => SetProperty(ref snackBarMessage, value);
        }

        private bool snackBar_IsOpen;
        public bool SnackBar_IsOpen {
            get => snackBar_IsOpen;
            set => SetProperty(ref snackBar_IsOpen, value);
        }

        #endregion

        private IAPIManager APIManager;

        private ILogger Logger;

        private IDialogService DialogService;

        private ISettingsManager SettingsManager;

        public LoginViewModel(IAPIManager _APIManager, ILoggerService loggerService, IDialogService dialogService, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<LoginViewModel>();
            APIManager = _APIManager;
            DialogService = dialogService;
            SettingsManager = settingsManager;

            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
        }

        private async void OnSignUpClicked(object obj) {
            // redirect to sign up page
            await Shell.Current.GoToAsync(PageNames.SignUpPage);
        }

        private async void OnLoginClicked(object obj)
        {
            SnackBarMessage = "Please wait. Logging in...";
            SnackBar_IsOpen = true;

            try {
                bool success = await APIManager.Login(email, password);
                if (success) {
                    await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
                }
                else {
                    SnackBar_IsOpen = false;
                    await DialogService.ShowMessage("Wrong login credentials!", "Access denied", "Ok", null);
                }
            } catch (CryptographicException) {
                // delete stored aes key
                await SettingsManager.UpdateCurrentSettings(currentSettings => {
                    currentSettings.AesKey = null;
                    currentSettings.AesIV = null;
                    return currentSettings;
                });

                await DialogService.ShowMessage("An error occured. Reconnecting to basestation...", "Info", "Ok", null);

                // exchange keys again
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.ConnectingPage));
            }
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
