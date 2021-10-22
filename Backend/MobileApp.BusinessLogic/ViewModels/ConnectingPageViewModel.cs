using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class ConnectingPageViewModel : BaseViewModel {

        private string status = "Status: -";
        public string Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        private ILogger Logger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private IDialogService DialogService;

        private ICloseApplicationService CloseApplicationService;

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public ConnectingPageViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IAesKeyExchangeManager aesKeyExchangeManager,
            IDialogService dialogService, ICloseApplicationService closeApplicationService, IBasestationFinderManager basestationFinderManager) {
            Logger = loggerService.GetLogger<ConnectingPageViewModel>();
            AesKeyExchangeManager = aesKeyExchangeManager;
            DialogService = dialogService;
            CloseApplicationService = closeApplicationService;
            BasestationFinderManager = basestationFinderManager;

            _ = BeginConnect();
        }

        ~ConnectingPageViewModel() {
            cancellationToken.Cancel();
        }

        public async Task BeginConnect() {
            // get basestation ip
            Status = "Status: Searching for a local basestation...";
            var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();

            if (baseStationFound) {
                Status = "Status: Exchanging keys...";
                Logger.Info($"[BeginConnect]Trying to get aes key from server.");
                var success = await AesKeyExchangeManager.Start(cancellationToken.Token);

                if (!success) {
                    await DialogService.ShowMessage("Could not connect to basestation!", "Error", "Close app", () => {
                        CloseApplicationService.CloseApplication();
                    });
                }
                else {
                    // redirect to login page
                    await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
                }

            }
            else {
                await DialogService.ShowMessage("Could not find a basestation in the local network!", "Error", "Close app", () => {
                    CloseApplicationService.CloseApplication();
                });
            }
        }
    }
}
