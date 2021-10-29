using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    [QueryProperty(nameof(SGModuleDetailViewModel.ItemId), nameof(ItemId))]
    public class SGModuleDetailViewModel : BaseViewModel {

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

        private string connectedToActor;
        public string ConnectedToActor {
            get => connectedToActor;
            set => SetProperty(ref connectedToActor, value);
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

        public SGModuleDetailViewModel(IDialogService dialogService) {
            DialogService = dialogService;

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
                ConnectedToActor = "Test1";
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
            //// save page information in cache
            //Guid storageId = Guid.NewGuid();
            //CachePageData.Store(storageId, this);

            //// open valve select page and pass storageId
            //await Shell.Current.GoToAsync($"{PageNames.SelectValvePage}?{nameof(SelectValvePageViewModel.AddModulePageStorageId)}={storageId}");
        }

        async void RemoveValveTapped(ModuleInfoDto obj) {
            throw new NotImplementedException();
        }
    }
}
