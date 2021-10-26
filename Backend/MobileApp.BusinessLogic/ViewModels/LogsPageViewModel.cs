using System.Threading.Tasks;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;

namespace MobileApp.BusinessLogic.ViewModels {
    public class LogsPageViewModel : BaseViewModel {

        private string logs = "Loading logs...";
        public string Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        private ILoggerService LoggerService;

        private IFileStorage FileStorage;

        public LogsPageViewModel(ILoggerService loggerService, IFileStorage fileStorage) {
            LoggerService = loggerService;
            FileStorage = fileStorage;

            _ = loadLogs();
        }

        private async Task loadLogs() {
            Logs = await FileStorage.ReadAsString(LoggerService.GetLogFilePath(allLogsFile: false));
        }
    }
}
