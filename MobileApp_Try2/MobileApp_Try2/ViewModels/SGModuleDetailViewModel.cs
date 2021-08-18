using System;
using System.Diagnostics;
using MobileApp_Try2.Views;
using Xamarin.Forms;

namespace MobileApp_Try2.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class SGModuleDetailViewModel : BaseViewModel
    {
        private string itemId;
        private string name;
        private string type;
        private string isOnline;
        private string measuredValue;
        private string id;
        private bool isRemoveButtonEnabled;
        private string connectedToActor;
        private bool isSelectActorVisible;
        private string lastUpdated;

        public Command RemoveCommand { get; }

        public SGModuleDetailViewModel() {
            Title = "Module Info";
            RemoveCommand = new Command(OnRemoveClicked);
        }

        public string Id 
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        public string IsOnline {
            get => isOnline;
            set => SetProperty(ref isOnline, value);
        }

        public string MeasuredValue {
            get => measuredValue;
            set => SetProperty(ref measuredValue, value);
        }

        public string LastUpdated {
            get => lastUpdated;
            set => SetProperty(ref lastUpdated, value);
        }

        public string ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
                LoadItemId(value);
            }
        }

        public bool IsRemoveButtonEnabled {
            get => isRemoveButtonEnabled;
            set => SetProperty(ref isRemoveButtonEnabled, value);
        }

        public bool IsSelectActorVisible {
            get => isSelectActorVisible;
            set => SetProperty(ref isSelectActorVisible, value);
        }

        public string ConnectedToActor {
            get => connectedToActor;
            set => SetProperty(ref connectedToActor, value);
        }

        public string ModuleImagePath {
            get { return "undraw_tabs"; }
        }

        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await ModulesDataStore.GetItemAsync(itemId);
                Id = item.Id;
                Name = item.Name;
                Type = item.Type;
                ConnectedToActor = "Test1";
                IsOnline = item.IsOnline.ToString();
                MeasuredValue = item.MeasuredValue;
                LastUpdated = DateTime.Now.ToString();

                if (item._Type == Models.SGModuleType.MAINSTATION)
                    IsRemoveButtonEnabled = false;
                else
                    IsRemoveButtonEnabled = true;

                if (item._Type == Models.SGModuleType.SENSOR)
                    IsSelectActorVisible = true;
                else
                    IsSelectActorVisible = false;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        async void OnRemoveClicked(object obj) {
            await ModulesDataStore.DeleteItemAsync(id);
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
    }
}
