using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications.Cryptography;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(ConnectToWlanPageViewModel.SelectedWlan_CacheId), nameof(SelectedWlan_CacheId))]
    [QueryProperty(nameof(ConnectToWlanPageViewModel.NavigationString), nameof(NavigationString))]
    public class ConnectToWlanPageViewModel : BaseViewModel {

        private string selectedWlan_CacheId = string.Empty;
        public string SelectedWlan_CacheId {
            get {
                return selectedWlan_CacheId;
            }
            set {
                selectedWlan_CacheId = value;
                loadWlanInfo();
            }
        }

        public string NavigationString { get; set; }

        #region page elements
        public ICommand AbortCommand { get; }
        public ICommand ConnectCommand { get; }

        private string wlanPassword;
        public string WlanPassword {
            get {
                return wlanPassword;
            }
            set {
                SetProperty(ref wlanPassword, value);
            }
        }

        private string ssid_text;
        public string SSID_Text {
            get {
                return ssid_text;
            }
            set {
                SetProperty(ref ssid_text, value);
            }
        }

        #region Snack bar elements

        private string snackBarMessage;
        public string SnackBarMessage {
            get => snackBarMessage;
            set => SetProperty(ref snackBarMessage, value);
        }

        private bool snackBar_IsOpen;
        public bool SnackBar_IsOpen {
            get => snackBar_IsOpen;
            set => SetProperty(ref snackBar_IsOpen, value);
        }

        #endregion
        #endregion


        private WlanInfoDto _wlanInfo;

        private bool _connecting;


        private ICachePageDataService CachePageDataService;

        private IDialogService DialogService;

        private ISettingsManager SettingsManager;

        private IAesEncrypterDecrypter AesEncrypterDecrypter;

        private ICommandManager CommandManager;

        public ConnectToWlanPageViewModel(ICachePageDataService cachePageDataService, IDialogService dialogService, ISettingsManager settingsManager,
            IAesEncrypterDecrypter aesEncrypterDecrypter, ICommandManager commandManager) {
            CachePageDataService = cachePageDataService;
            DialogService = dialogService;
            SettingsManager = settingsManager;
            AesEncrypterDecrypter = aesEncrypterDecrypter;
            CommandManager = commandManager;

            AbortCommand = new Command(OnAbortTapped);
            ConnectCommand = new Command(async () => await OnConnectTapped());

            _connecting = false;
        }

        async Task OnConnectTapped() {
            if (!_connecting) {
                try {
                    _connecting = true;
                    SnackBarMessage = "Please wait. Connecting...";
                    SnackBar_IsOpen = true;

                    if (string.IsNullOrEmpty(NavigationString)) {
                        NavigationString = PageNames.LoginPage;
                    }
                    if (string.IsNullOrEmpty(SelectedWlan_CacheId) || _wlanInfo == null) {
                        await DialogService.ShowMessage("Selected wlan did not get passed to this page!", "ERROR", "Ok", null);
                        await Shell.Current.GoToAsync(NavigationString);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(WlanPassword)) {
                        await DialogService.ShowMessage("Please enter a valid password first!", "ERROR", "Ok", null);
                        return;
                    }

                    // encrypt password
                    var settings = await SettingsManager.GetApplicationSettings();
                    if (settings.AesIV != null && settings.AesKey != null) {
                        _wlanInfo.EncryptedPassword = AesEncrypterDecrypter.Encrypt(WlanPassword);

                        // pass the information to the basestation
                        var success = await CommandManager.ConnectToWlan(_wlanInfo);
                        if (success) {
                            await DialogService.ShowMessage($"Successfully connected basestation to wlan {_wlanInfo.Ssid}!\n" +
                                $"Please restart the app and reconnect to your basestation.", "Info", "Ok", null);
                            await Shell.Current.GoToAsync(NavigationString);
                        }
                        else {
                            await DialogService.ShowMessage("Could not connect to wlan.", "Error", "Ok", null);
                        }
                    }
                    else {
                        await DialogService.ShowMessage("No keys got exchanged with the server.", "Error", "Ok", null);
                        await Shell.Current.GoToAsync(NavigationString);
                    }
                }
                finally {
                    SnackBar_IsOpen = false;
                    _connecting = false;
                }
            }
            else {
                await DialogService.ShowMessage("Already connecting...\nPlease wait.", "Info", "Ok", null);
            }
        }

        async void OnAbortTapped() {
            if (string.IsNullOrEmpty(NavigationString)) {
                NavigationString = PageNames.LoginPage;
            }

            await Shell.Current.GoToAsync(NavigationString);
        }

        void loadWlanInfo() {
            // get object to store the connect information in (ssid, password)
            _wlanInfo = CachePageDataService.RemoveFromStore(Guid.Parse(SelectedWlan_CacheId)) as WlanInfoDto;

            // display ssid
            SSID_Text = $"{_wlanInfo.Ssid}:";
        }
    }
}
