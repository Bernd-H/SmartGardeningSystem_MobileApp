using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.BusinessLogic.Managers;
using MobileApp.Common;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using NLog;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class MainPageViewModel : BaseViewModel {

        #region Log page properties (swipe left)

        private string logs = "";
        public string Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        #endregion
        public ICommand OpenWebCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ViewLogsPageCommand { get; }


        private ModuleInfoDto _selectedItem;

        public ObservableCollection<ModuleInfoDto> Items { get; }

        public Command LoadItemsCommand { get; }

        public Command AddItemCommand { get; }

        public Command SettingsCommand { get; }

        public Command<ModuleInfoDto> ItemTapped { get; }

        private string status = "-";
        public string Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        private string temperature = "-";
        public string Temperature {
            get => temperature;
            set => SetProperty(ref temperature, value);
        }

        private string systemUpTime = "-";
        public string SystemUpTime {
            get => systemUpTime;
            set => SetProperty(ref systemUpTime, value);
        }

        private ILogger Logger;

        private ILoggerService LoggerService;

        private IDialogService DialogService;

        private IDataStore<ModuleInfo> ModuleRepository;

        private IAPIManager APIManager;

        private ISettingsManager SettingsManager;

        private IFileStorage FileStorage;

        private ICommandManager CommandManager;

        public MainPageViewModel(ILoggerService loggerService, IDialogService dialogService, IDataStore<ModuleInfo> moduleRepository, IAPIManager _APIManager,
            ISettingsManager settingsManager, IFileStorage fileStorage, ICommandManager commandManager) {
            Logger = loggerService.GetLogger<MainPageViewModel>();
            LoggerService = loggerService;
            DialogService = dialogService;
            ModuleRepository = moduleRepository;
            APIManager = _APIManager;
            SettingsManager = settingsManager;
            FileStorage = fileStorage;
            CommandManager = commandManager;

            // module items
            Items = new ObservableCollection<ModuleInfoDto>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<ModuleInfoDto>(OnItemSelected);
            AddItemCommand = new Command(OnAddItem);

            // other commands
            SettingsCommand = new Command(OnSettingsTapped);
            HelpCommand = new Command(OnHelpTapped);
            StartCommand = new Command(OnStartTapped);
            StopCommand = new Command(OnStopTapped);
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://github.com/Bernd-H/SmartGardeningSystem"));
            ViewLogsPageCommand = new Command(OnViewLogsPageTapped);
        }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                // load system status info
                var sysStatus = await APIManager.GetSystemStatus();
                Status = sysStatus.WateringStatus;
                Temperature = sysStatus.Temperature.ToString() + "°C";
                SystemUpTime = sysStatus.SystemUpTime;

                // load modules
                Items.Clear();
                var items = (await ModuleRepository.GetItemsAsync(true)).ToDtos();
                foreach (var item in items) {
                    Items.Add(item);
                }
            }
            catch (UnauthorizedAccessException) {
                // stored token invalid -> login again
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.LoginPage));
            }
            catch (Exception ex) {
                Logger.Error(ex, $"[ExecuteLoadItemsCommand]An error occured while getting modules.");
            }
            finally {
                IsBusy = false;
            }
        }

        public async void OnAppearing() {
            IsBusy = true;
            SelectedItem = null;

            #region Show connect to wlan page

            //if (SettingsManager.GetRuntimeVariables().LoadedMainPageFirstTime) {
            //    // update runtime variable
            //    SettingsManager.UpdateCurrentRuntimeVariables((rv) => {
            //        rv.LoadedMainPageFirstTime = false;
            //        return rv;
            //    });

            //    try {
            //        if (await APIManager.IsBasestationConnectedToWlan() == false) {
            //            // show page to select a wlan to connect the basestation to
            //            string navigationString = PageNames.GetNavigationString(PageNames.MainPage);
            //            await Shell.Current.GoToAsync($"{PageNames.SelectWlanPage}?{nameof(SelectWlanPageViewModel.NavigationString)}={navigationString}");
            //        }
            //    }
            //    catch (UnauthorizedAccessException) {
            //        // stored token invalid -> login again
            //        // login page will get called in ExecuteLoadItemsCommand() after trying to get all modules
            //    }
            //}

            #endregion
        }

        public ModuleInfoDto SelectedItem {
            get => _selectedItem;
            set {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnAddItem(object obj) {
            Logger.Info($"[OnAddItem]Loading WaitingForNewModulePage...");
            await Shell.Current.GoToAsync(PageNames.WaitingForNewModulePage);
        }

        async void OnHelpTapped(object obj) {
            Logger.Info($"[OnHelpTapped]Loading help page...");
            await Shell.Current.GoToAsync(PageNames.HelpPage);
        }

        async void OnSettingsTapped(object obj) {
            Logger.Info($"[OnSettingsTapped]Loading settings page...");
            await Shell.Current.GoToAsync(PageNames.SettingsPage);
        }

        async void OnStartTapped(object obj) {
            bool success = await CommandManager.StartAutomaticIrrigation();
            if (success) {
                var refreshTask = ExecuteLoadItemsCommand(); // to refresh the system status info
                await DialogService.ShowMessage("Successfully started the automatic irrigation.", "Info", "Ok", null);
                await refreshTask;
            }
            else {
                await DialogService.ShowMessage("Something went wrong while trying to start the automatic irrigation.\n" +
                    "If the SystemStatus is \"ManualIrrigation\" the stop the manual irrigation with Settings->Stop-manual-irrigation first.", "Error", "Ok", null);
            }
        }

        async void OnStopTapped(object obj) {
            bool success = await CommandManager.StopAutomaticIrrigation();
            if (success) {
                var refreshTask = ExecuteLoadItemsCommand(); // to refresh the system status info
                await DialogService.ShowMessage("Successfully stopped the automatic irrigation.", "Info", "Ok", null);
                await refreshTask;
            }
            else {
                await DialogService.ShowMessage("Something went wrong while trying to stop the automatic irrigation.\n" +
                    "If the SystemStatus is \"ManualIrrigation\" the stop the manual irrigation with Settings->Stop-manual-irrigation first.", "Error", "Ok", null);
            }
        }

        async void OnItemSelected(ModuleInfoDto item) {
            if (item == null)
                return;

            Logger.Info($"[OnItemSelected]Loading detail page for module with id {item.ModuleId}...");

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{PageNames.SGModuleDetailPage}?{nameof(SGModuleDetailViewModel.ItemId)}={item.ModuleId}");
        }

        async void OnViewLogsPageTapped(object obj) {
            Logger.Info($"[OnViewLogsPageTapped]Loading logs page...");
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }
    }
}
