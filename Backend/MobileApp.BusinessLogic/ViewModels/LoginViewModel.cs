using System.Reflection;
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


        public LoginViewModel(IAPIManager _APIManager, IAesKeyExchangeManager aesKeyExchangeManager, ILoggerService loggerService,
             IDialogService dialogService) {
            Logger = loggerService.GetLogger<LoginViewModel>();
            APIManager = _APIManager;

            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            SnackBarMessage = "Please wait. Logging in...";
            SnackBar_IsOpen = true;

            bool success = await APIManager.Login(email, password);
            if (success) {
                await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
            } else {
                SnackBar_IsOpen = false;
                await DialogService.ShowMessage("Wrong login credentials!", "Access denied", "Ok", null);
            }
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
