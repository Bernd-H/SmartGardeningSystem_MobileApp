using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(SelectWlanPageViewModel.NavigationString), nameof(NavigationString))]
    public class SelectWlanPageViewModel : BaseViewModel {

        public string NavigationString { get; set; }

        public ObservableCollection<WlanInfoDto> Wlans { get; }

        public Command<WlanInfoDto> ItemTapped { get; }

        public ICommand LoadItemsCommand { get; }

        public ICommand NoWlanCommand { get; }


        private ICachePageDataService CachePageDataService;

        private IDataStore<WlanInfoDto> WlansDataStore;

        public SelectWlanPageViewModel(ICachePageDataService cachePageDataService, IDataStore<WlanInfoDto> wlansDataStore) {
            CachePageDataService = cachePageDataService;
            WlansDataStore = wlansDataStore;

            Wlans = new ObservableCollection<WlanInfoDto>();
            ItemTapped = new Command<WlanInfoDto>(OnItemSelected);
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            NoWlanCommand = new Command(NoWlanTapped);
        }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                Wlans.Clear();
                var items = await WlansDataStore.GetItemsAsync(forceRefresh: true);
                foreach (var item in items ?? Enumerable.Empty<WlanInfoDto>()) {
                    Wlans.Add(item);
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
        }

        async void OnItemSelected(WlanInfoDto item) {
            if (item == null)
                return;

            // store selected wlan in cache, so that the connectToWlanPage can access it
            var cacheStorageId = Guid.NewGuid();
            CachePageDataService.Store(cacheStorageId, item);

            await Shell.Current.GoToAsync($"{PageNames.ConnectToWlanPage}?{nameof(ConnectToWlanPageViewModel.SelectedWlan_CacheId)}={cacheStorageId}&" +
                $"{nameof(ConnectToWlanPageViewModel.NavigationString)}={NavigationString}");
        }

        async void NoWlanTapped() {
            if (string.IsNullOrEmpty(NavigationString)) {
                NavigationString = PageNames.LoginPage;
            }

            await Shell.Current.GoToAsync(NavigationString);
        }
    }
}
