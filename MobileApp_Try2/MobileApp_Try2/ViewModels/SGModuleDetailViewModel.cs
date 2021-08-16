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
        private string infoText;
        private string isOnline;
        private string measuredValue;
        private string id;

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

        public string InfoText
        {
            get => infoText;
            set => SetProperty(ref infoText, value);
        }

        public string IsOnline {
            get => infoText;
            set => SetProperty(ref isOnline, value);
        }

        public string MeasuredValue {
            get => infoText;
            set => SetProperty(ref measuredValue, value);
        }

        public string LastUpdated {
            get {
                return "Last updated: " + DateTime.Now.ToString();
            }
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
                InfoText = item.InfoText;
                IsOnline = item.IsOnline.ToString();
                MeasuredValue = item.MeasuredValue;
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
