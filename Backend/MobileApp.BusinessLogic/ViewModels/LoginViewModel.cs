using System.Reflection;
using System.Threading.Tasks;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }


        private string email;
        public string Email {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password;
        public string Password {
            get => email;
            set => SetProperty(ref email, value);
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

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private ILogger Logger;

        private ISettingsManager SettingsManager;


        private Task getAesServerKeyTask;

        public LoginViewModel(IAPIManager _APIManager, IAesKeyExchangeManager aesKeyExchangeManager, ILoggerService loggerService, ISettingsManager settingsManager) {
            Logger = loggerService.GetLogger<LoginViewModel>();
            APIManager = _APIManager;
            AesKeyExchangeManager = aesKeyExchangeManager;
            SettingsManager = settingsManager;

            LoginCommand = new Command(OnLoginClicked);

            SnackBarMessage = "Test123!";
            SnackBar_IsOpen = true;

            var settings = SettingsManager.GetApplicationSettings().Result;
            if (settings.AesKey == null || settings.AesIV == null) {
                getAesServerKeyTask = AesKeyExchangeManager.Start();
            }
        }

        private async void OnLoginClicked(object obj)
        {
            Task LoginTask = APIManager.Login(email, password);

            // load screen

            LoginTask.Wait();
            await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
