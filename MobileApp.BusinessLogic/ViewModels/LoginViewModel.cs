using System.Reflection;
using MobileApp.Common;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {

            await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
        }

        public string LoginImagePath {
            get { return "undraw_authentication_fsn5"; }
        }
    }
}
