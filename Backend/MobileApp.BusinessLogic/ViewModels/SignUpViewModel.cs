using System.Security.Cryptography;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class SignUpViewModel : BaseViewModel {
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

        private string confirmPassword;
        public string ConfirmPassword {
            get => confirmPassword;
            set => SetProperty(ref confirmPassword, value);
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

        public SignUpViewModel(IAPIManager _APIManager, ILoggerService loggerService, IDialogService dialogService, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<SignUpViewModel>();
            APIManager = _APIManager;
            DialogService = dialogService;
            SettingsManager = settingsManager;

            SignUpCommand = new Command(OnSignUpClicked);
        }

        private async void OnSignUpClicked(object obj) {
            // check entered email and password
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword)) {
                await DialogService.ShowMessage("Please fill out all entries.", "Error", "Ok", null);
                return;
            }
            if (Password != ConfirmPassword) {
                await DialogService.ShowMessage("Entered passwords are not equal.", "Error", "Ok", null);
                return;
            }

            SnackBarMessage = "Please wait. Forwoarding data to server...";
            SnackBar_IsOpen = true;

            try {
                bool success = await APIManager.Register(email, password);
                if (success) {
                    await DialogService.ShowMessage("Your account creation was successful.", "Info", "Ok", null);
                    await Shell.Current.GoToAsync($"//{PageNames.LoginPage}");
                }
                else {
                    SnackBar_IsOpen = false;
                    await DialogService.ShowMessage("Something went wrong!\nPossible reason: Your email may be already registered.", "Error", "Ok", null);
                }
            }
            catch (CryptographicException) {
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

        public string RegisterImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
