using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class ConnectingPageViewModel : BaseViewModel {

        #region Log page properties (swipe left)

        private string logs = "";
        public string Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        #endregion

        private string status = "-";
        public string Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        private bool activityIndicatorIsVisible = true;
        public bool ActivityIndicatorIsVisible {
            get { return activityIndicatorIsVisible; }
            set => SetProperty(ref activityIndicatorIsVisible, value);
        }


        public ICommand ViewLogsPageCommand { get; }


        private ILoggerService LoggerService;

        private ILogger ConnectingPageLogger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private IDialogService DialogService;

        private ICloseApplicationService CloseApplicationService;

        private IAPIManager APIManager;

        private IFileStorage FileStorage;

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public ConnectingPageViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IAesKeyExchangeManager aesKeyExchangeManager,
            IDialogService dialogService, ICloseApplicationService closeApplicationService, IBasestationFinderManager basestationFinderManager, IAPIManager _APIManager,
            IFileStorage fileStorage) {
            LoggerService = loggerService;
            ConnectingPageLogger = loggerService.GetLogger<ConnectingPageViewModel>();
            AesKeyExchangeManager = aesKeyExchangeManager;
            DialogService = dialogService;
            CloseApplicationService = closeApplicationService;
            BasestationFinderManager = basestationFinderManager;
            APIManager = _APIManager;
            FileStorage = fileStorage;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);

            _ = BeginConnect();
        }

        ~ConnectingPageViewModel() {
            cancellationToken.Cancel();
        }

        public Task LoadLogs() {
            return Task.Run(async () => {
                ConnectingPageLogger.Trace($"[LoadLogs]Thread: {Thread.CurrentThread.ManagedThreadId}.");
                LoggerService.GetLogger<LogsPageViewModel>().Info($"[LoadLogs]Loading logs.");
                var logs = await FileStorage.ReadAsString(LoggerService.GetLogFilePath(allLogsFile: false));

                // show logs
                Logs = logs;
            });
        }


        Task BeginConnect() {
            return Task.Run(async () => {
                ConnectingPageLogger.Trace($"[BeginConnect]Thread: {Thread.CurrentThread.ManagedThreadId}.");

                // get basestation ip
                Status = "Searching for a local basestation...";
                var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();
                //ConnectingPageLogger.Warn("[BeginConnect]Mocking BasestationIP for test reasons.");
                //var baseStationFound = true;
                //await Common.Configuration.IoC.Get<ISettingsManager>().UpdateCurrentSettings(currentSettings => {
                //    currentSettings.BaseStationIP = "10.0.2.2";
                //    return currentSettings;
                //});

                if (baseStationFound) {
                    Status = "Exchanging keys...";
                    ConnectingPageLogger.Info($"[BeginConnect]Trying to get aes key from server.");
                    var success = await AesKeyExchangeManager.Start(cancellationToken.Token);

                    if (!success) {
                        ActivityIndicatorIsVisible = false;
                        await DialogService.ShowMessage("Could not connect to basestation!", "Error", "Ok", () => {
                            Status = "Exchanging a key with the basestation failed! Please restart the application.";
                            //CloseApplicationService.CloseApplication();
                        });
                    }
                    else {
                        // redirect to login page
                        await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
                    }

                }
                else {
                    ConnectingPageLogger.Info("Could not find a basestation in the local network!");
                    ActivityIndicatorIsVisible = false;
                    await DialogService.ShowMessage("Could not find a basestation in the local network!", "Error", "Ok", () => {
                        //CloseApplicationService.CloseApplication();
                        Status = "Find a basestation failed! Please restart the application.";
                    });
                }
            });
        }

        async void OnViewLogsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }
    }
}
