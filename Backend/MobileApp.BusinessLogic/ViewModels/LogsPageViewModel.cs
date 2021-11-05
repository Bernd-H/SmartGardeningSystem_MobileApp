using System.Threading.Tasks;
using System.Windows.Input;
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

        public ICommand LoadLogsCommand { get; }

        private bool isButtonVisible = true;
        public bool IsButtonVisible {
            get => isButtonVisible;
            set => SetProperty(ref isButtonVisible, value);
        }

        private ILoggerService LoggerService;

        private IFileStorage FileStorage;

        public LogsPageViewModel(ILoggerService loggerService, IFileStorage fileStorage) {
            LoggerService = loggerService;
            FileStorage = fileStorage;

            LoadLogsCommand = new Command(loadLogs);
        }

        async void loadLogs() {
            LoggerService.GetLogger<LogsPageViewModel>().Info($"[loadLogs]Loading logs.");
            var logs = await FileStorage.ReadAsString(LoggerService.GetLogFilePath(allLogsFile: false));

            // remove button
            IsButtonVisible = false;

            // show logs
            Logs = logs;
        }
    }
}
