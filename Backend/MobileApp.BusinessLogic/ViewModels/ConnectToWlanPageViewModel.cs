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

        private WlanInfoDto wlanInfo;


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
        }

        async Task OnConnectTapped() {
            if (string.IsNullOrEmpty(NavigationString)) {
                NavigationString = PageNames.LoginPage;
            }

            if (string.IsNullOrEmpty(SelectedWlan_CacheId) || wlanInfo == null) {
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
                wlanInfo.EncryptedPassword = AesEncrypterDecrypter.Encrypt(WlanPassword);

                // pass the information to the basestation
                var success = await CommandManager.ConnectToWlan(wlanInfo);
                if (success) {
                    await DialogService.ShowMessage($"Successfully connected basestation to wlan {wlanInfo.Ssid}!", "Info", "Ok", null);
                    await Shell.Current.GoToAsync(NavigationString);
                } else {
                    await DialogService.ShowMessage("Could not connect to wlan.", "Error", "Ok", null);
                }
            }
            else {
                await DialogService.ShowMessage("No keys got exchanged with the server.", "Error", "Ok", null);
                await Shell.Current.GoToAsync(NavigationString);
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
            wlanInfo = CachePageDataService.RemoveFromStore(Guid.Parse(SelectedWlan_CacheId)) as WlanInfoDto;

            // display ssid
            SSID_Text = $"{wlanInfo.Ssid}:";
        }
    }
}
