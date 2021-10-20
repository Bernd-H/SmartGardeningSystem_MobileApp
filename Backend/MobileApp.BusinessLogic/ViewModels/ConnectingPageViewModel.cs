using System;
using System.Collections.Generic;
using System.Text;
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

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private IDialogService DialogService;

        private ICloseApplicationService CloseApplicationService;

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public ConnectingPageViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IAesKeyExchangeManager aesKeyExchangeManager,
            IDialogService dialogService, ICloseApplicationService closeApplicationService) {
            Logger = loggerService.GetLogger<ConnectingPageViewModel>();
            AesKeyExchangeManager = aesKeyExchangeManager;
            DialogService = dialogService;
            CloseApplicationService = closeApplicationService;

            _ = BeginConnect();
        }

        ~ConnectingPageViewModel() {
            cancellationToken.Cancel();
        }

        public async Task BeginConnect() {
            Logger.Info($"[BeginConnect]Trying to get aes key from server.");
            var success = await AesKeyExchangeManager.Start(cancellationToken.Token);

            if (!success) {
                await DialogService.ShowMessage("Could not connect to basestation!", "Error", "Close app", () => {
                    CloseApplicationService.CloseApplication();
                });
            } else {
                // redirect to login page
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
            }
        }
    }
}
