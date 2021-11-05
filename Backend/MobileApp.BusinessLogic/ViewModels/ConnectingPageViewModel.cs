using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class ConnectingPageViewModel : BaseViewModel {

        private string status = "-";
        public string Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        public ICommand ViewLogsPageCommand { get; }


        private ILogger Logger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private IDialogService DialogService;

        private ICloseApplicationService CloseApplicationService;

        private IAPIManager APIManager;

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public ConnectingPageViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IAesKeyExchangeManager aesKeyExchangeManager,
            IDialogService dialogService, ICloseApplicationService closeApplicationService, IBasestationFinderManager basestationFinderManager, IAPIManager _APIManager) {
            Logger = loggerService.GetLogger<ConnectingPageViewModel>();
            AesKeyExchangeManager = aesKeyExchangeManager;
            DialogService = dialogService;
            CloseApplicationService = closeApplicationService;
            BasestationFinderManager = basestationFinderManager;
            APIManager = _APIManager;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);

            _ = BeginConnect();
        }

        ~ConnectingPageViewModel() {
            cancellationToken.Cancel();
        }

        async Task BeginConnect() {
            // get basestation ip
            Status = "Searching for a local basestation...";
            var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();

            if (baseStationFound) {
                Status = "Exchanging keys...";
                Logger.Info($"[BeginConnect]Trying to get aes key from server.");
                var success = await AesKeyExchangeManager.Start(cancellationToken.Token);

                if (!success) {
                    await DialogService.ShowMessage("Could not connect to basestation!", "Error", "Ok", () => {
                        Status = "Exchanging a key with the basestation failed! Please restart the application.";
                        //CloseApplicationService.CloseApplication();
                    });
                }
                else {
                    //// check if basestation is connected to a wlan
                    //if (await APIManager.IsBasestationConnectedToWlan()) {
                        // redirect to login page
                        await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
                    //}
                    //else {
                    //    await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.SelectWlanPage));
                    //}
                }

            }
            else {
                await DialogService.ShowMessage("Could not find a basestation in the local network!", "Error", "Ok", () => {
                    //CloseApplicationService.CloseApplication();
                    Status = "Find a basestation failed! Please restart the application.";
                });
            }
        }

        async void OnViewLogsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }
    }
}
