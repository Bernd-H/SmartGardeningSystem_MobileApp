using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
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

        #region Show logs content page properties

        private string logs;
        public string Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }


        private bool updateViewProperty;
        public bool UpdateViewProperty {
            get { return updateViewProperty; }
            set { SetProperty(ref updateViewProperty, value); }
        }

        #endregion

        private ILogger Logger;

        private ILoggerService LoggerService;

        private IAPIManager APIManager;

        private IDialogService DialogService;

        private ISettingsManager SettingsManager;

        private IFileStorage FileStorage;

        public LoginViewModel(ILoggerService loggerService, IAPIManager _APIManager, IDialogService dialogService, ISettingsManager settingsManager,
            IFileStorage fileStorage) {
            Logger = loggerService.GetLogger<LoginViewModel>();
            LoggerService = loggerService;
            APIManager = _APIManager;
            DialogService = dialogService;
            SettingsManager = settingsManager;
            FileStorage = fileStorage;

            LoginCommand = new Command(OnLoginClicked);
            SignUpCommand = new Command(OnSignUpClicked);
        }

        private async void OnSignUpClicked(object obj) {
            // redirect to sign up page
            Logger.Info($"[OnSignUpClicked]Loading sign up page.");
            await Shell.Current.GoToAsync(PageNames.SignUpPage);
        }

        private async void OnLoginClicked(object obj)
        {
            SnackBarMessage = "Please wait. Logging in...";
            SnackBar_IsOpen = true;

            try {
                bool success = await APIManager.Login(email, password);
                SnackBar_IsOpen = false;

                if (success) {
                    Logger.Info($"[OnLoginClicked]Loading main page.");
                    await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
                }
                else {
                    await DialogService.ShowMessage("Wrong login credentials!", "Access denied", "Ok", null);
                }
            } catch (CryptographicException) {
                SnackBar_IsOpen = false;

                // delete stored aes key
                await SettingsManager.UpdateCurrentSettings(currentSettings => {
                    currentSettings.AesKey = null;
                    currentSettings.AesIV = null;
                    return currentSettings;
                });

                await DialogService.ShowMessage("An error occured. Reconnecting to basestation...", "Info", "Ok", null);

                // exchange keys again
                Logger.Info($"[OnLoginClicked]Loading connecting page, to exchange important keys again.");
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.ConnectingPage));
            }
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }

        /// <summary>
        /// Logging in this method is not allowed.
        /// (Would create an infinite loop, because LoadLogs() gets called automatically, when 
        /// someone logs -> LoggerService.AddEventHandler(....) in constructor....)
        /// </summary>
        public async void LoadLogs(object sender, EventArgs eventArgs) {
            var logsFilePath = LoggerService.GetLogFilePath(allLogsFile: false);
            string logs;
            if (File.Exists(logsFilePath)) {
                //LoggerService.GetLogger<LogsPageViewModel>().Trace($"[LoadLogs]Loading logs.");
                logs = await FileStorage.ReadAsString(logsFilePath);
            }else {
                logs = "No log file found.";
            }

            // show logs
            Logs = logs;

            // to update the view...
            UpdateViewProperty = !UpdateViewProperty;
        }
    }
}
