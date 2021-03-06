using System;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class SettingsPageViewModel : BaseViewModel {
        //public ICommand ManualControlCommand { get; set; }

        public ICommand StartManualIrrigationCommand { get; set; }

        public ICommand StopManualIrrigationCommand { get; set; }

        public ICommand AccountSettingsCommand { get; set; }

        public ICommand ChangeWlanCommand { get; set; }

        public ICommand BackCommand { get; set; }

        public ICommand DisconnectFromWlanCommand { get; set; }

        public ICommand OpenLogsPageCommand { get; set; }


        private IDialogService DialogService;

        private ICommandManager CommandManager;

        private ILogger Logger;

        public SettingsPageViewModel(ILoggerService loggerService, ICommandManager commandManager, IDialogService dialogService) {
            Logger = loggerService.GetLogger<SettingsPageViewModel>();
            CommandManager = commandManager;
            DialogService = dialogService;

            StartManualIrrigationCommand = new Command(OnStartManualIrrigationTapped);
            StopManualIrrigationCommand = new Command(OnStopManualIrrigationTapped);
            AccountSettingsCommand = new Command(AccountSettingsTapped);
            ChangeWlanCommand = new Command(ChangeWlanTapped);
            BackCommand = new Command(OnBackTapped);
            DisconnectFromWlanCommand = new Command(OnDisconnectFromWlanTapped);
            OpenLogsPageCommand = new Command(OnOpenLogsPageTapped);
        }

        private async void OnBackTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
        }

        private async void ChangeWlanTapped(object obj) {
            await Shell.Current.GoToAsync($"{PageNames.SelectWlanPage}?{nameof(SelectWlanPageViewModel.NavigationString)}={PageNames.SettingsPage}");
        }

        private async void AccountSettingsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.AccountPage);
        }

        private async void OnDisconnectFromWlanTapped(object obj) {
            var waitMessage = DialogService.ShowMessage("Please wait... The initiated process can take some time.", "Info", "Ok", null);
            bool success = await CommandManager.DisconnectFromWlan();

            await waitMessage;

            if (success) {
                await DialogService.ShowMessage("Basestation got successfully disconnected from the wlan." +
                    "\nPlease connect your phone to the access point, which the basestation will open in a few minutes." +
                    "\nThen restart the mobile app and login to your basestation.", "Info", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("Something went wrong, while disconnecting the basestation from the wlan.", "Error", "Ok", null);
            }
        }

        private async void OnStartManualIrrigationTapped(object obj) {
            var waitMessage = DialogService.ShowMessage("Please wait... The initiated process can take some time.", "Info", "Ok", null);
            var timespan = TimeSpan.FromHours(2);
            bool success = await CommandManager.StartManualIrrigation(timespan);
            await waitMessage;

            if (success) {
                await DialogService.ShowMessage("Opened all valves that are enabled for manual irrigation.\n" +
                    $"These vavles will close automatically after {timespan.TotalHours}h.", "Info", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("Something went wrong while trying to start the manual irrigation.", "Error", "Ok", null);
            }
        }

        private async void OnStopManualIrrigationTapped(object obj) {
            var waitMessage = DialogService.ShowMessage("Please wait... The initiated process can take some time.", "Info", "Ok", null);
            bool success = await CommandManager.StopManualIrrigation();
            await waitMessage;

            if (success) {
                await DialogService.ShowMessage("Closed all valves which are enabled for the manual irrigation successfully.", "Info", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("Something went wrong while trying to stop the manual irrigation.", "Error", "Ok", null);
            }
        }

        private async void OnOpenLogsPageTapped(object obj) {
            Logger.Info($"[OnOpenLogsPageTapped]Loading logs page.");
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }
    }
}
