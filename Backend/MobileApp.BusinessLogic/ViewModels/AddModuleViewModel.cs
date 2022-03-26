using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty("DataInCacheId", nameof(DataInCacheId))]
    [QueryProperty(nameof(AddModuleViewModel.ModuleId), nameof(ModuleId))]
    public class AddModuleViewModel : BaseViewModel, IValvesListViewModel {
        /// <summary>
        /// Query property. Defines under what id AddModuleViewModel properties got stored.
        /// </summary>
        private string dataInCacheId = string.Empty;
        public string DataInCacheId {
            get {
                return dataInCacheId;
            }
            set {
                dataInCacheId = value;

                // check if page got called from selectValvePage
                if (DataInCacheId != string.Empty) {
                    // load stored data
                    var storedData = CachePageData.RemoveFromStore(Guid.Parse(DataInCacheId)) as AddModuleViewModel;

                    // set properties
                    Name = storedData.Name;

                    ModuleId = storedData.ModuleId; // will also set booleans AddingASensor and AddingAValve
                    WateringSetting_SliderValue = storedData.WateringSetting_SliderValue;
                    UpdateLinkedValvesCollectionView(storedData.LinkedValves);

                    WateringMethod_PickerIndex = storedData.WateringMethod_PickerIndex;
                }
            }
        }

        /// <summary>
        /// Query property. Used to update the right module, after Name, Valves and so on got set.
        /// </summary>
        private string moduleId = string.Empty;
        public string ModuleId {
            get {
                return moduleId;
            }
            set {
                moduleId = value;
                // No refresh nedded. The WaitingForNewModulePageViewModel class refreshes the internal cache
                var m = ModuleRepository.GetItemAsync(Utils.ConvertHexToByte(moduleId), forceRefresh: false).Result;
                AddingASensor = m.ModuleType == ModuleType.Sensor;
            }
        }


        private string name;
        public string Name {
            get {
                return name;
            }
            set {
                SetProperty(ref name, value);
            }
        }

        private bool addingASensor;
        public bool AddingASensor {
            get {
                return addingASensor;
            }
            set {
                SetProperty(ref addingASensor, value);
                AddingAValve = !value;
            }
        }

        private bool addingAValve;
        public bool AddingAValve {
            get {
                return addingAValve;
            }
            set {
                SetProperty(ref addingAValve, value);
            }
        }

        public string AddModuleImagePath {
            get { return "undraw_Mind_map_re_nlb6"; }
        }

        public ICommand SaveCommand { get; }

        public ICommand BackCommand { get; }

        #region properties concerning adding a sensor

        public ObservableCollection<ModuleInfoDto> LinkedValves { get; set; }

        public ICommand AddCorrespondingValveCommand { get; }

        public Command<ModuleInfoDto> RemoveValveFromModuleCommand { get; }

        public double WateringSetting_SliderValue { get; set; } // 0 / 1 / 2

        #endregion

        #region properties concerning adding a valve

        public int WateringMethod_PickerIndex { get; set; }

        public IList<string> WateringMethods { get; }

        #endregion

        private ILogger Logger;

        private ICachePageDataService CachePageData;

        private IDialogService DialogService;

        private IDataStore<ModuleInfo> ModuleRepository;

        public AddModuleViewModel(ICachePageDataService cachePageDataService, IDialogService dialogService, IDataStore<ModuleInfo> moduleRepository,
            ILoggerService loggerService) {
            Logger = loggerService.GetLogger<AddModuleViewModel>();
            CachePageData = cachePageDataService;
            DialogService = dialogService;
            ModuleRepository = moduleRepository;

            SaveCommand = new Command(SaveTapped);
            BackCommand = new Command(BackTapped);
            AddCorrespondingValveCommand = new Command(AddLinkedValveTapped);
            RemoveValveFromModuleCommand = new Command<ModuleInfoDto>(RemoveValveTapped);
            LinkedValves = new ObservableCollection<ModuleInfoDto>();
            WateringSetting_SliderValue = 0;
            WateringMethod_PickerIndex = -1;
            WateringMethods = new List<string>() { Common.Models.Enums.WateringMethods.DROPBYDROP, Common.Models.Enums.WateringMethods.SPRINKLER };
        }

        async void BackTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
        }

        async void SaveTapped(object obj) {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name) || (AddingASensor && LinkedValves.Count == 0) ||
                (!AddingASensor && WateringMethod_PickerIndex == -1)) {
                await DialogService.ShowMessage("Can not process data. Please make sure that everything is set properly.", "Error", "Ok", null);
                return;
            }

            if (string.IsNullOrWhiteSpace(ModuleId)) {
                await DialogService.ShowMessage("Fatal error! ModuleId did not get set in background.\nRestart the application and try again.", "Error", "Ok", null);
                return;
            }

            var m = await ModuleRepository.GetItemAsync<byte>(Utils.ConvertHexToByte(ModuleId));


            // process data
            m.Name = Name;
            m.ModuleType = AddingASensor ? ModuleType.Sensor : ModuleType.Valve;

            // add type specific data
            if (!AddingASensor) {
                var wateringMethod = new Common.Models.Enums.WateringMethods(WateringMethods[WateringMethod_PickerIndex]);

                // moduleInfo.wateringMethod....
            }
            else {
                var linkedValveIds = new List<byte>();
                foreach (var valve in LinkedValves) {
                    linkedValveIds.Add(Utils.ConvertHexToByte(valve.ModuleId));
                }

                m.AssociatedModules = linkedValveIds;
            }

            // send data to api
            bool success = await ModuleRepository.UpdateItemAsync(m);
            if (!success) {
                await DialogService.ShowMessage("An error accrued while updating the module.", "Error", "Ok", null);
            }
            else {
                await DialogService.ShowMessage("Successfully added the new module.", "Info", "Ok", null);

                // navigate
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
            }
        }

        async void AddLinkedValveTapped(object obj) {
            // save page information in cache
            Guid storageId = Guid.NewGuid();
            CachePageData.Store(storageId, this);

            // open valve select page and pass storageId and page to navigate to after
            await Shell.Current.GoToAsync($"{PageNames.SelectValvePage}?{nameof(SelectValvePageViewModel.AddModulePageStorageId)}={storageId}&{nameof(SelectValvePageViewModel.NavigationString)}={PageNames.AddModulePage}");
        }

        private void RemoveValveTapped(ModuleInfoDto module) {
            LinkedValves.Remove(module);
        }

        private void UpdateLinkedValvesCollectionView(IEnumerable<ModuleInfoDto> valves) {
            LinkedValves.Clear();
            foreach (var valve in valves) {
                LinkedValves.Add(valve);
            }
        }
    }
}
