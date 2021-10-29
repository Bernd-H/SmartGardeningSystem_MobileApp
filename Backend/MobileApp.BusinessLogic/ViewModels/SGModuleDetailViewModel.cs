using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(SGModuleDetailViewModel.ItemId), nameof(ItemId))]
    [QueryProperty("DataInCacheId", nameof(DataInCacheId))]
    public class SGModuleDetailViewModel : BaseViewModel, IValvesListViewModel {

        /// <summary>
        /// Query property. Defines what module should get displayed.
        /// </summary>
        private string itemId;
        public string ItemId {
            get {
                return itemId;
            }
            set {
                itemId = value;
                LoadItemId(value);
                FillLinkedValvesList(value);
            }
        }

        /// <summary>
        /// Query property. Defines under what id SGModuleDetailViewModel properties got stored.
        /// </summary>
        private string dataInCacheId;
        public string DataInCacheId {
            get {
                return dataInCacheId;
            }
            set {
                dataInCacheId = value;

                if (dataInCacheId != string.Empty) {
                    // load cached data and set properties
                    var cachedData = CachePageDataService.RemoveFromStore(Guid.Parse(dataInCacheId)) as SGModuleDetailViewModel;
                    itemId = cachedData.ItemId;

                    Id = cachedData.Id;
                    Name = cachedData.Name;
                    Type = cachedData.Type;
                    IsOnline = cachedData.IsOnline;
                    MeasuredValue = cachedData.MeasuredValue;
                    LastUpdated = cachedData.LastUpdated;
                    IsRemoveButtonEnabled = cachedData.IsRemoveButtonEnabled;
                    IsSelectActorVisible = cachedData.IsSelectActorVisible;
                    IsWateringSettingVisible = cachedData.IsWateringSettingVisible;

                    UpdateLinkedValvesCollectionView(cachedData.LinkedValves);
                    ProcessChangedCorrespondingValveList();
                }
            }
        }

        #region binded properties

        private string id;
        public string Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string name;
        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string type;
        public string Type {
            get => type;
            set => SetProperty(ref type, value);
        }

        private string isOnline;
        public string IsOnline {
            get => isOnline;
            set => SetProperty(ref isOnline, value);
        }

        private string measuredValue;
        public string MeasuredValue {
            get => measuredValue;
            set => SetProperty(ref measuredValue, value);
        }

        private string lastUpdated;
        public string LastUpdated {
            get => lastUpdated;
            set => SetProperty(ref lastUpdated, value);
        }

        private bool isRemoveButtonEnabled;
        public bool IsRemoveButtonEnabled {
            get => isRemoveButtonEnabled;
            set => SetProperty(ref isRemoveButtonEnabled, value);
        }

        private bool isSelectActorVisible;
        public bool IsSelectActorVisible {
            get => isSelectActorVisible;
            set => SetProperty(ref isSelectActorVisible, value);
        }

        private bool isWateringSettingVisible;
        public bool IsWateringSettingVisible {
            get => isWateringSettingVisible;
            set => SetProperty(ref isWateringSettingVisible, value);
        }

        public string ModuleImagePath {
            get { return "undraw_tabs"; }
        }

        public Command RemoveCommand { get; }

        #endregion

        #region show / add / remove linked valves properties

        public ObservableCollection<ModuleInfoDto> LinkedValves { get; set; }

        public ICommand AddCorrespondingValveCommand { get; }

        public Command<ModuleInfoDto> RemoveValveFromModuleCommand { get; }

        #endregion


        private IDialogService DialogService;

        private ICachePageDataService CachePageDataService;

        public SGModuleDetailViewModel(IDialogService dialogService, ICachePageDataService cachePageDataService) {
            DialogService = dialogService;
            CachePageDataService = cachePageDataService;

            Title = "Module Info";
            RemoveCommand = new Command(OnRemoveClicked);
            LinkedValves = new ObservableCollection<ModuleInfoDto>();
            AddCorrespondingValveCommand = new Command(AddCorrespondingValveTapped);
            RemoveValveFromModuleCommand = new Command<ModuleInfoDto>(RemoveValveTapped);
        }

        public async void LoadItemId(string itemId) {
            try {
                var item = await ModulesDataStore.GetItemAsync(itemId);
                Id = item.Id.ToString();
                Name = item.Name;
                Type = item.Type.Value;
                //ConnectedToActor = "Test1";
                IsOnline = item.IsOnline.ToString();
                MeasuredValue = item.MeasuredValue;
                LastUpdated = DateTime.Now.ToString();

                if (item.Type.Value.Equals(ModuleTypes.MAINSTATION))
                    IsRemoveButtonEnabled = false;
                else
                    IsRemoveButtonEnabled = true;

                if (item.Type.Value.Equals(ModuleTypes.SENSOR)) {
                    IsSelectActorVisible = true;
                    IsWateringSettingVisible = true;
                }
                else {
                    IsSelectActorVisible = false;
                    IsWateringSettingVisible = false;
                }
            }
            catch (Exception) {
                await DialogService.ShowMessage("An error accrued while loading a module.", "Error", "Ok", async () => {
                    await Shell.Current.GoToAsync("..");
                });
            }
        }

        async void FillLinkedValvesList(string itemId) {
            try {
                var item = await ModulesDataStore.GetItemAsync(itemId);
                if (item.Type.Value.Equals(ModuleTypes.SENSOR)) {
                    var correspondingValvesIds = item.CorrespondingValves;

                    // fill list with valves
                    foreach (var id in correspondingValvesIds) {
                        LinkedValves.Add(await ModulesDataStore.GetItemAsync(id.ToString()));
                    }
                }
            }
            catch (Exception) {
                await DialogService.ShowMessage("An error accrued while loading a module.", "Error", "Ok", async () => {
                    await Shell.Current.GoToAsync("..");
                });
            }
        }

        async void OnRemoveClicked(object obj) {
            await ModulesDataStore.DeleteItemAsync(id);
            await Shell.Current.GoToAsync($"//{PageNames.MainPage}");
        }

        async void AddCorrespondingValveTapped(object obj) {
            // let user select a valve
            // save page information in cache
            Guid storageId = Guid.NewGuid();
            CachePageDataService.Store(storageId, this);

            // open valve select page and pass storageId and page to navigate to after
            await Shell.Current.GoToAsync($"{PageNames.SelectValvePage}?{nameof(SelectValvePageViewModel.AddModulePageStorageId)}={storageId}&{nameof(SelectValvePageViewModel.NavigationString)}={PageNames.SGModuleDetailPage}");
        }

        async void ProcessChangedCorrespondingValveList() {
            Task userDialog = DialogService.ShowMessage("Updating module. Please wait...", "Info", "Ok", null);

            // update module via api


            await userDialog;
        }

        async void RemoveValveTapped(ModuleInfoDto obj) {
            Task userDialog = DialogService.ShowMessage("Updating module. Please wait...", "Info", "Ok", null);

            // update module via api

            // if success...
            //UpdateLinkedValvesCollectionView()

            await userDialog;
        }

        void UpdateLinkedValvesCollectionView(IEnumerable<ModuleInfoDto> valves) {
            LinkedValves.Clear();
            foreach (var valve in valves) {
                LinkedValves.Add(valve);
            }
        }
    }
}
