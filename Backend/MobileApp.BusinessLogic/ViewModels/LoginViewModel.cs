using System.Reflection;
using System.Threading.Tasks;
using MobileApp.Common;
using MobileApp.Common.Specifications.Managers;
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

        private IAPIManager APIManager;

        public LoginViewModel(IAPIManager _APIManager)
        {
            APIManager = _APIManager;

            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            Task LoginTask = APIManager.Login(email, password);

            // load screen

            await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
