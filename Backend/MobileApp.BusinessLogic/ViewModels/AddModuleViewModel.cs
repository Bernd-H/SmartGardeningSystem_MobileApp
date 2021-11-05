using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty("DataInCacheId", nameof(DataInCacheId))]
    [QueryProperty(nameof(AddModuleViewModel.AddingASensor), nameof(AddingASensor))]
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

                    AddingASensor = storedData.AddingASensor;
                    WateringSetting_SliderValue = storedData.WateringSetting_SliderValue;
                    UpdateLinkedValvesCollectionView(storedData.LinkedValves);

                    WateringMethod_PickerIndex = storedData.WateringMethod_PickerIndex;
                }
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

        public Command SaveCommand { get; }

        public string AddModuleImagePath {
            get { return "undraw_Mind_map_re_nlb6"; }
        }


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


        private ICachePageDataService CachePageData;

        private IDialogService DialogService;

        private IDataStore<ModuleInfoDto> ModuleRepository;

        public AddModuleViewModel(ICachePageDataService cachePageDataService, IDialogService dialogService, IDataStore<ModuleInfoDto> moduleRepository) {
            CachePageData = cachePageDataService;
            DialogService = dialogService;
            ModuleRepository = moduleRepository;

            SaveCommand = new Command(SaveTapped);
            AddCorrespondingValveCommand = new Command(AddLinkedValveTapped);
            RemoveValveFromModuleCommand = new Command<ModuleInfoDto>(RemoveValveTapped);
            LinkedValves = new ObservableCollection<ModuleInfoDto>();
            WateringSetting_SliderValue = 0;
            WateringMethod_PickerIndex = -1;
            WateringMethods = new List<string>() { Common.Models.Enums.WateringMethods.DROPBYDROP, Common.Models.Enums.WateringMethods.SPRINKLER };
        }

        async void SaveTapped(object obj) {
            // check if everything is set
            bool everythingSet = true;
            if (string.IsNullOrEmpty(Name) || string.IsNullOrWhiteSpace(Name))
                everythingSet = false;
            if (AddingASensor) {
                if (LinkedValves.Count == 0)
                    everythingSet = false;
            } else {
                if (WateringMethod_PickerIndex == -1)
                    everythingSet = false;
            }

            if (everythingSet) {
                ModuleInfoDto moduleData = new ModuleInfoDto();

                // process data
                moduleData.Id = Guid.NewGuid();
                moduleData.Name = Name;
                moduleData.InformationTimestamp = DateTime.Now;
                if (AddingASensor)
                    moduleData.Type = new ModuleTypes(ModuleTypes.SENSOR);
                else
                    moduleData.Type = new ModuleTypes(ModuleTypes.VALVE);

                // add type specific data
                if (!AddingASensor) {
                    var wateringMethod = new Common.Models.Enums.WateringMethods(WateringMethods[WateringMethod_PickerIndex]);

                    // moduleInfoDto.wateringMethod....
                } else {
                    var linkedValveIds = new List<Guid>();
                    foreach (var valve in LinkedValves) {
                        linkedValveIds.Add(valve.Id);
                    }

                    moduleData.CorrespondingValves = linkedValveIds;
                }

                // send data to api
                bool success = await ModuleRepository.AddItemAsync(moduleData);
                if (!success) {
                    await DialogService.ShowMessage("An error accrued while adding module to repository.", "Error", "Ok", null);
                }

                // navigate
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
            } else {
                await DialogService.ShowMessage("Can not process data. Please make sure that everything is set properly.", "Error", "Ok", null);
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
