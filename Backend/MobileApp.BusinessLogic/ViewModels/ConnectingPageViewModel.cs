using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
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

        private bool updateViewProperty;
        /// <summary>
        /// To fix a bug. When an element in the main content page gets updated, then
        /// the second content page (CarouselPage) gets also updated...
        /// (Used in LoadLogs())
        /// </summary>
        public bool UpdateViewProperty {
            get { return updateViewProperty; }
            set { SetProperty(ref updateViewProperty, value); }
        }

        private bool deleteSettingsButtonEnabled = false;
        public bool DeleteSettingsButtonEnabled {
            get { return deleteSettingsButtonEnabled; }
            set { SetProperty(ref deleteSettingsButtonEnabled, value); }
        }

        private bool reconnectButtonEnabled = false;
        public bool ReconnectButtonEnabled {
            get { return  reconnectButtonEnabled; }
            set { SetProperty(ref reconnectButtonEnabled, value); }
        }

        public string ConnectImagePath {
            get { return "undraw_connected_world_wuay"; }
        }

        public ICommand ViewLogsPageCommand { get; }

        public ICommand DeleteSettingsCommand { get; }

        public ICommand ReconnectCommand { get; }


        private ILoggerService LoggerService;

        private ILogger Logger;

        private IBasestationFinderManager BasestationFinderManager;

        private IAesKeyExchangeManager AesKeyExchangeManager;

        private IDialogService DialogService;

        private ICloseApplicationService CloseApplicationService;

        private IAPIManager APIManager;

        private IFileStorage FileStorage;

        private ISettingsManager SettingsManager;

        private IRelayManager RelayManager;


        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private Task beginConnectTask = null;

        public ConnectingPageViewModel(ILoggerService loggerService, ISettingsManager settingsManager, IAesKeyExchangeManager aesKeyExchangeManager,
            IDialogService dialogService, ICloseApplicationService closeApplicationService, IBasestationFinderManager basestationFinderManager, IAPIManager _APIManager,
            IFileStorage fileStorage, IRelayManager relayManager) {
            LoggerService = loggerService;
            Logger = loggerService.GetLogger<ConnectingPageViewModel>();
            AesKeyExchangeManager = aesKeyExchangeManager;
            DialogService = dialogService;
            CloseApplicationService = closeApplicationService;
            BasestationFinderManager = basestationFinderManager;
            APIManager = _APIManager;
            FileStorage = fileStorage;
            SettingsManager = settingsManager;
            RelayManager = relayManager;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);
            DeleteSettingsCommand = new Command(OnDeleteSettingsTapped);
            ReconnectCommand = new Command(OnReconnectTapped);

            LoggerService.AddEventHandler(new EventHandler(async (s, e) => await LoadLogs(s,e)));

            if (beginConnectTask == null) {
                beginConnectTask = BeginConnect();
            }
        }

        ~ConnectingPageViewModel() {
            cancellationToken.Cancel();
        }

        /// <summary>
        /// Logging in this method is not allowed.
        /// (Would create an infinite loop, because LoadLogs() gets called automatically, when 
        /// someone logs -> LoggerService.AddEventHandler(....) in constructor....)
        /// </summary>
        public async Task LoadLogs(object sender, EventArgs eventArgs) {
            var logsFilePath = LoggerService.GetLogFilePath(allLogsFile: false);
            string logs;
            if (File.Exists(logsFilePath)) {
                //LoggerService.GetLogger<LogsPageViewModel>().Trace($"[LoadLogs]Loading logs.");
                logs = await FileStorage.ReadAsString(logsFilePath);
            }
            else {
                logs = "No log file found.";
            }

            // show logs
            Logs = logs;

            // to update the view...
            UpdateViewProperty = !UpdateViewProperty;
        }


        Task BeginConnect() {
            return Task.Run(async () => {
                Logger.Debug($"[BeginConnect]Thread: {Thread.CurrentThread.ManagedThreadId}.");
                ActivityIndicatorIsVisible = true;

                // get basestation ip
                Status = "Searching for a local basestation...";
                var baseStationFound = await BasestationFinderManager.FindLocalBaseStation();
                //Logger.Warn("[BeginConnect]Mocking BasestationIP for test reasons.");
                //var baseStationFound = false;
                //var baseStationFound = true;
                //await Common.Configuration.IoC.Get<ISettingsManager>().UpdateCurrentSettings(currentSettings => {
                //    currentSettings.BaseStationIP = "10.0.2.2";
                //    return currentSettings;
                //});
                //await Common.Configuration.IoC.Get<ISettingsManager>().UpdateCurrentSettings(currentSettings => {
                //    currentSettings.AesKey = null;
                //    return currentSettings;
                //});

                if (!baseStationFound) {
                    // try to establish a connection over the external server
                    Status = "Trying to connect to the basestation over the external server...";
                    baseStationFound = await RelayManager.ConnectToTheBasestation(CancellationToken.None);
                }

                if (baseStationFound) {
                    bool success = false;
                    var settings = await SettingsManager.GetApplicationSettings();
                    if (settings.AesKey == null || settings.AesIV == null) {
                        // perform key exchange
                        Status = "Exchanging keys...";
                        Logger.Info($"[BeginConnect]Trying to get aes key from server.");
                        success = await AesKeyExchangeManager.Start(cancellationToken.Token);
                    }
                    else {
                        // key already exchanged and available
                        success = true;
                    }

                    if (!success) {
                        ActivityIndicatorIsVisible = false;
                        await DialogService.ShowMessage("Could not connect to basestation!", "Error", "Ok", () => {
                            Status = "Exchanging a key with the basestation failed! Please restart the application.";
                            DeleteSettingsButtonEnabled = true;
                            ReconnectButtonEnabled = true;
                            //CloseApplicationService.CloseApplication();
                        });
                    }
                    else if (settings.SessionAPIToken?.IsTokenValid() ?? false) {
                        // redirect to main page
                        await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
                    }
                    else {
                        // redirect to login page
                        await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
                    }
                }
                else {
                    Logger.Info("Finding a basestation failed!");
                    ActivityIndicatorIsVisible = false;
                    await DialogService.ShowMessage("Could not find a basestation in the local network!", "Error", "Ok", () => {
                        Status = "No basestation found! Please restart the application.";
                        DeleteSettingsButtonEnabled = true;
                        ReconnectButtonEnabled = true;
                        //CloseApplicationService.CloseApplication();
                    });
                }
            });
        }

        async void OnViewLogsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }

        async void OnDeleteSettingsTapped(object obj) {
            await SettingsManager.UpdateCurrentSettings((currentSettings) => {
                return ApplicationSettingsDto.GetStandardSettings();
            });
            await DialogService.ShowMessage("Settings successfully deleted.", "Info", "Ok", () => {
                DeleteSettingsButtonEnabled = false;
            });
        }  
        
        async void OnReconnectTapped(object obj) {
            SettingsManager.accessedSecureStorage = false;
            DeleteSettingsButtonEnabled = false;
            ReconnectButtonEnabled = false;
            await BeginConnect();
        }
    }
}
