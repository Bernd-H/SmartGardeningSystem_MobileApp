using System;
using System.Threading.Tasks;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.Services {
    public class DialogService : IDialogService {

        public DialogService() {

        }

        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback) {
            return Task.Run(() => {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert(
                title,
                message,
                buttonText);
                });
                if (afterHideCallback != null) {
                    afterHideCallback();
                }
            });
        }

        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback) {
            return Task.Run(() => {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert(title, error.Message, buttonText);
                });

                if (afterHideCallback != null) {
                    afterHideCallback();
                }
            });
        }

        public Task ShowMessage(string message, string title) {
            return Task.Run(() => {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                });
            });
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback) {
            return Task.Run(() => {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert(
                    title,
                    message,
                    buttonText);
                });

                if (afterHideCallback != null) {
                    afterHideCallback();
                }
            });
        }

        public Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText, Action<bool> afterHideCallback) {
            return Task<bool>.Run(() => {
                bool result = false;
                Device.BeginInvokeOnMainThread(async () => {
                    result = await Application.Current.MainPage.DisplayAlert(
                    title,
                    message,
                    buttonConfirmText,
                    buttonCancelText);
                });


                if (afterHideCallback != null) {
                    afterHideCallback(result);
                }

                return result;
            });
        }

        public Task ShowMessageBox(string message, string title) {
            return Task.Run(() => {
                Device.BeginInvokeOnMainThread(async () => {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                });
            });
        }
    }
}
