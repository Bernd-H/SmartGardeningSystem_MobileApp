using System.IO;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class LogsPageViewModel : BaseViewModel {

        private string logs = "";
        public string Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        private bool isButtonVisible = true;
        public bool IsButtonVisible {
            get => isButtonVisible;
            set => SetProperty(ref isButtonVisible, value);
        }

        public ICommand LoadLogsCommand { get; }

        public ICommand BackCommand { get; set; }


        private ILoggerService LoggerService;

        private IFileStorage FileStorage;

        public LogsPageViewModel(ILoggerService loggerService, IFileStorage fileStorage) {
            LoggerService = loggerService;
            FileStorage = fileStorage;

            LoadLogsCommand = new Command(loadLogs);
            BackCommand = new Command(OnBackTapped);
        }

        /// <summary>
        /// Logging in this method is not allowed.
        /// (Would create an infinite loop, because LoadLogs() gets called automatically, when 
        /// someone logs -> LoggerService.AddEventHandler(....) in constructor....)
        /// </summary>
        async void loadLogs() {
            var logsFilePath = LoggerService.GetLogFilePath(allLogsFile: false);
            string logs;
            if (File.Exists(logsFilePath)) {
                logs = await FileStorage.ReadAsString(logsFilePath);
            }
            else {
                logs = "No log file found.";
            }

            // remove button
            IsButtonVisible = false;

            // show logs
            Logs = logs;
        }

        async void OnBackTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.SettingsPage);
        }
    }
}
