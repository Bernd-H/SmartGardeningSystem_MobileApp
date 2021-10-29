using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class MainPageViewModel : BaseViewModel {

        public ICommand OpenWebCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ViewLogsPageCommand { get; }


        private ModuleInfoDto _selectedItem;

        public ObservableCollection<ModuleInfoDto> Items { get; }

        public Command LoadItemsCommand { get; }

        public Command AddItemCommand { get; }

        public Command AccountCommand { get; }

        public Command<ModuleInfoDto> ItemTapped { get; }


        private IDialogService DialogService;

        public MainPageViewModel(IDialogService dialogService) {
            DialogService = dialogService;

            // module items
            Items = new ObservableCollection<ModuleInfoDto>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<ModuleInfoDto>(OnItemSelected);
            AddItemCommand = new Command(OnAddItem);

            // other commands
            AccountCommand = new Command(OnAccountTapped);
            HelpCommand = new Command(OnHelpTapped);
            StartCommand = new Command(OnStartTapped);
            StopCommand = new Command(OnStopTapped);
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://www.djcodex.com"));
            ViewLogsPageCommand = new Command(OnViewLogsPageTapped);
        }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                Items.Clear();
                var items = await ModulesDataStore.GetItemsAsync(true);
                foreach (var item in items) {
                    Items.Add(item);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
            finally {
                IsBusy = false;
            }
        }

        public void OnAppearing() {
            IsBusy = true;
            SelectedItem = null;
        }

        public ModuleInfoDto SelectedItem {
            get => _selectedItem;
            set {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnAddItem(object obj) {
            await Shell.Current.GoToAsync(PageNames.WaitingForNewModulePage);
        }

        async void OnHelpTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.HelpPage);
        }

        async void OnAccountTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.AccountPage);
        }

        async void OnStartTapped(object obj) {
            await DialogService.ShowMessage("Start tapped.", "Info", "Ok", null);
        }

        async void OnStopTapped(object obj) {
            await DialogService.ShowMessage("Stop tapped.", "Info", "Ok", null);
        }

        async void OnItemSelected(ModuleInfoDto item) {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{PageNames.SGModuleDetailPage}?{nameof(SGModuleDetailViewModel.ItemId)}={item.Id}");
        }

        async void OnViewLogsPageTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }
    }
}
