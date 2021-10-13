using System.Reflection;
using MobileApp.Common;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        public Command LoggoutCommand { get; }
        public Command ChangePasswordCommand { get; }
        public Command ChangeEmailCommand { get; }

        public AccountViewModel()
        {
            LoggoutCommand = new Command(OnLoggoutClicked);
            ChangePasswordCommand = new Command(OnChangePassword);
            ChangeEmailCommand = new Command(OnChangeEmail);
        }

        private async void OnLoggoutClicked(object obj)
        {
            await Shell.Current.GoToAsync($"//{PageNames.LoginPage}");
        }

        private async void OnChangePassword(object obj) {
            //await DisplayAlert("Alert", "You have been alerted", "OK");
        }
        private async void OnChangeEmail(object obj) {
            //await DisplayAlert("Alert", "You have been alerted", "OK");
        }
    }
}
