using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using NLog;
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
                moduleId = Utils.ConvertHexToByte(itemId);
                LoadItemId(value);
                FillLinkedValvesList();
            }
        }

        private byte moduleId;

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

        private string lastUpdated;
        public string LastUpdated {
            get => lastUpdated;
            set => SetProperty(ref lastUpdated, value);
        }

        private bool isRemoveButtonEnabled = false;
        public bool IsRemoveButtonEnabled {
            get => isRemoveButtonEnabled;
            set => SetProperty(ref isRemoveButtonEnabled, value);
        }

        private bool isSelectActorVisible = false;
        public bool IsSelectActorVisible {
            get => isSelectActorVisible;
            set => SetProperty(ref isSelectActorVisible, value);
        }

        private bool isWateringSettingVisible = false;
        public bool IsWateringSettingVisible {
            get => isWateringSettingVisible;
            set => SetProperty(ref isWateringSettingVisible, value);
        }

        public string ModuleImagePath {
            get { return "undraw_tabs"; }
        }


        private bool manualIrrigationEnabled;

        public bool ManualIrrigationEnabled {
            get => manualIrrigationEnabled;
            set => SetProperty(ref manualIrrigationEnabled, value);
        }

        private bool isAValve = false;

        public bool IsAValve {
            get => isAValve;
            set => SetProperty(ref isAValve, value);
        }

        public ICommand RemoveCommand { get; }

        public ICommand BackCommand { get; }

        #endregion

        #region show / add / remove linked valves properties

        public ObservableCollection<ModuleInfoDto> LinkedValves { get; set; }

        public ICommand AddCorrespondingValveCommand { get; }

        public Command<ModuleInfoDto> RemoveValveFromModuleCommand { get; }

        #endregion


        private ILogger Logger;

        private IDialogService DialogService;

        private ICachePageDataService CachePageDataService;

        private IDataStore<ModuleInfo> ModuleRepository;

        private IAPIManager APIManager;

        public SGModuleDetailViewModel(LoggerService loggerService, IDialogService dialogService, ICachePageDataService cachePageDataService,
            IDataStore<ModuleInfo> moduleRepository, IAPIManager _APIManager) {
            Logger = loggerService.GetLogger<SGModuleDetailViewModel>();
            DialogService = dialogService;
            CachePageDataService = cachePageDataService;
            ModuleRepository = moduleRepository;
            APIManager = _APIManager;

            Title = "Module Info";
            RemoveCommand = new Command(OnRemoveClicked);
            BackCommand = new Command(OnBackTapped);
            LinkedValves = new ObservableCollection<ModuleInfoDto>();
            AddCorrespondingValveCommand = new Command(AddCorrespondingValveTapped);
            RemoveValveFromModuleCommand = new Command<ModuleInfoDto>(RemoveValveTapped);
        }

        public async void LoadItemId(string itemId) {
            try {
                var item = (await ModuleRepository.GetItemAsync(moduleId))?.ToDto();
                Id = item.ModuleId;
                Name = item.Name;
                Type = item.ModuleTypeName;
                LastUpdated = item.InformationTimestamp.ToString();
                ManualIrrigationEnabled = item.EnabledForManualIrrigation;

                if (item.ModuleTypeName == ModuleTypeNames.VALVE) {
                    IsRemoveButtonEnabled = true;
                    IsAValve = true;
                }
                else if (item.ModuleTypeName == ModuleTypeNames.SENSOR) {
                    IsSelectActorVisible = true;
                    IsWateringSettingVisible = true;
                    IsRemoveButtonEnabled = true;
                }
                else {
                    IsRemoveButtonEnabled = false;
                    IsAValve = false;
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

        public async Task SetManualIrrigationSettings(bool enabled) {
            var m = await ModuleRepository.GetItemAsync(moduleId);
            if (ManualIrrigationEnabled == m.EnabledForManualIrrigation) {
                // this event got called when LoadItemId set the ManualIrrigationEnabled checkbox
                return;
            }

            Logger.Info($"[SetManualIrrigationSettings]Settings manual irrigation setting to {enabled} for module with id={ItemId}.");
            var module = ParseToModuleInfo(associatedModules: null); // null because it's a valve not a sensor
            
            bool success = await APIManager.UpdateModule(module);
            if (success) {
                await DialogService.ShowMessage("Module successfully updated.", "Info", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("An error occured while trying to update this setting.", "Error", "Ok", null);
                m = await ModuleRepository.GetItemAsync(moduleId);
                ManualIrrigationEnabled = m.EnabledForManualIrrigation;
            }
        }

        async void FillLinkedValvesList() {
            try {
                var item = await ModuleRepository.GetItemAsync(moduleId);
                if (item.ModuleType == ModuleType.Sensor) {
                    var correspondingValvesIds = item.AssociatedModules;

                    // fill list with valves
                    LinkedValves.Clear();
                    foreach (var id in correspondingValvesIds ?? Enumerable.Empty<byte>()) {
                        LinkedValves.Add((await ModuleRepository.GetItemAsync(id))?.ToDto());
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
            bool success = await ModuleRepository.DeleteItemAsync(moduleId);
            if (!success) {
                await DialogService.ShowMessage("Error while removing module!", "Error", "Ok", null);
            }

            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
        }

        async void OnBackTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
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

            // build associatedModules list
            var associatedModules = new List<byte>();
            foreach (var valve in LinkedValves) {
                associatedModules.Add(Utils.ConvertHexToByte(valve.ModuleId));
            }

            // update module via api
            bool success = await ModuleRepository.UpdateItemAsync(ParseToModuleInfo(associatedModules));

            await userDialog;

            // show success / fail
            if (!success) {
                await DialogService.ShowMessage("Updating module failed!", "Error", "Ok", null);
                // go back to main page
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
            }
        }

        async void RemoveValveTapped(ModuleInfoDto obj) {
            Task userDialog = DialogService.ShowMessage("Updating module. Please wait...", "Info", "Ok", null);

            // build associatedModules list without the module to remove
            var associatedModules = new List<byte>();
            foreach (var valve in LinkedValves) {
                if (valve.ModuleId != obj.ModuleId) {
                    associatedModules.Add(Utils.ConvertHexToByte(valve.ModuleId));
                }
            }

            // update module via api
            bool success = await ModuleRepository.UpdateItemAsync(ParseToModuleInfo(associatedModules));

            await userDialog;

            if (success) {
                await UpdateLinkedValvesCollectionView((await ModuleRepository.GetItemAsync(moduleId)).AssociatedModules);
                //await DialogService.ShowMessage("Module successfully updated!", "Info", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("Updating module failed!", "Error", "Ok", null);
            }
        }

        private void UpdateLinkedValvesCollectionView(IEnumerable<ModuleInfoDto> valves) {
            LinkedValves.Clear();
            foreach (var valve in valves) {
                LinkedValves.Add(valve);
            }
        }

        private async Task UpdateLinkedValvesCollectionView(IEnumerable<byte> valveIds) {
            LinkedValves.Clear();
            foreach (var valveId in valveIds) {
                var linkedModuleInformation = (await ModuleRepository.GetItemAsync(valveId))?.ToDto();
                LinkedValves.Add(linkedModuleInformation);
            }
        }

        /// <summary>
        /// Converts all properties shown in this view into a single object
        /// </summary>
        /// <param name="associatedModules">LinkedValveIds</param>
        private ModuleInfo ParseToModuleInfo(List<byte> associatedModules) {
            Logger.Info($"[ParseToModuleInfo]Parsing displayed module data to an ModuleInfo object.");
            var m = new ModuleInfoDto {
                ModuleId = ItemId,
                ModuleTypeName = Type,
                Name = Name,
                AssociatedModules = associatedModules,
                EnabledForManualIrrigation = ManualIrrigationEnabled
            };

            return m.FromDto();
        }
    }
}
