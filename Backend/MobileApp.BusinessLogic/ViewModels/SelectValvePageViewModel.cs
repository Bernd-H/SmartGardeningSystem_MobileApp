using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(SelectValvePageViewModel.AddModulePageStorageId), nameof(AddModulePageStorageId))]
    public class SelectValvePageViewModel : BaseViewModel {

        private string addModulePageStorageId = string.Empty;
        public string AddModulePageStorageId {
            get {
                return addModulePageStorageId;
            }
            set {
                addModulePageStorageId = value;
                _ = ExecuteLoadItemsCommand();
            }
        }

        //private ModuleInfoDto _selectedItem;
        //public ModuleInfoDto SelectedItem {
        //    get => _selectedItem;
        //    set {
        //        SetProperty(ref _selectedItem, value);
        //        OnItemSelected(value);
        //    }
        //}


        public ObservableCollection<ModuleInfoDto> Valves { get; }

        public Command<ModuleInfoDto> ItemTapped { get; }

        public ICommand LoadItemsCommand { get; }


        private ICachePageDataService CachePageDataService;

        public SelectValvePageViewModel(ICachePageDataService cachePageDataService) {
            Valves = new ObservableCollection<ModuleInfoDto>();
            ItemTapped = new Command<ModuleInfoDto>(OnItemSelected);
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            CachePageDataService = cachePageDataService;
        }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                if (AddModulePageStorageId != string.Empty) {
                    var alreadyLinkedValves = (CachePageDataService.GetFromStore(Guid.Parse(AddModulePageStorageId)) as AddModuleViewModel).LinkedValves.ToList();

                    Valves.Clear();
                    var items = await ModulesDataStore.GetItemsAsync(true);
                    foreach (var item in items) {
                        if (item.Type.Value == ModuleTypes.VALVE && alreadyLinkedValves.Find(m => m.Id == item.Id) == null) {
                            Valves.Add(item);
                        }
                    }
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
            //SelectedItem = null;
        }

        async void OnItemSelected(ModuleInfoDto item) {
            if (item == null)
                return;

            // update linked valve list in data cache
            CachePageDataService.UpdateCachedPageData(Guid.Parse(AddModulePageStorageId), data => {
                var pageViewModel = (AddModuleViewModel)data;
                pageViewModel.LinkedValves.Add(item);
                return pageViewModel;
            });

            await Shell.Current.GoToAsync($"{PageNames.AddModulePage}?{nameof(AddModuleViewModel.DataInCacheId)}={AddModulePageStorageId}");
        }
    }
}
