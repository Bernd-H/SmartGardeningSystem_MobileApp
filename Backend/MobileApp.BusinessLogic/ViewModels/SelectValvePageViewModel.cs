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
using MobileApp.Common.Specifications;
using System.Collections.Generic;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(SelectValvePageViewModel.AddModulePageStorageId), nameof(AddModulePageStorageId))]
    [QueryProperty(nameof(SelectValvePageViewModel.NavigationString), nameof(NavigationString))]
    public class SelectValvePageViewModel : BaseViewModel {

        private string addModulePageStorageId = string.Empty;
        public string AddModulePageStorageId {
            get {
                return addModulePageStorageId;
            }
            set {
                addModulePageStorageId = value;
            }
        }

        /// <summary>
        /// This navigation string will be used to navigate to the next page after an element got selected.
        /// </summary>
        public string NavigationString { get; set; }


        public ObservableCollection<ModuleInfoDto> Valves { get; }

        public Command<ModuleInfoDto> ItemTapped { get; }

        public ICommand LoadItemsCommand { get; }


        private ICachePageDataService CachePageDataService;

        private IDataStore<ModuleInfoDto> ModuleRepository;

        public SelectValvePageViewModel(ICachePageDataService cachePageDataService, IDataStore<ModuleInfoDto> moduleRepository) {
            CachePageDataService = cachePageDataService;
            ModuleRepository = moduleRepository;

            Valves = new ObservableCollection<ModuleInfoDto>();
            ItemTapped = new Command<ModuleInfoDto>(OnItemSelected);
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                if (AddModulePageStorageId != string.Empty) {
                    var alreadyLinkedValves = (CachePageDataService.GetFromStore(Guid.Parse(AddModulePageStorageId)) as IValvesListViewModel).LinkedValves.ToList();

                    Valves.Clear();
                    var items = await ModuleRepository.GetItemsAsync(true);
                    foreach (var item in items ?? Enumerable.Empty<ModuleInfoDto>()) {
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
        }

        async void OnItemSelected(ModuleInfoDto item) {
            if (item == null)
                return;

            // update linked valve list in data cache
            CachePageDataService.UpdateCachedPageData(Guid.Parse(AddModulePageStorageId), data => {
                var pageViewModel = (IValvesListViewModel)data;
                pageViewModel.LinkedValves.Add(item);
                return pageViewModel;
            });

            await Shell.Current.GoToAsync($"{NavigationString}?DataInCacheId={AddModulePageStorageId}");
        }
    }
}
