using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp_Try2.Models;
using MobileApp_Try2.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileApp_Try2.ViewModels {
    public class MainPageViewModel : BaseViewModel {
        public MainPageViewModel() {
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://www.djcodex.com"));
            AddModuleCommand = new Command(async () => await Shell.Current.GoToAsync("//LoginPage"));

            // module items
            Items = new ObservableCollection<SGModule>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<SGModule>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
            AccountCommand = new Command(OnAccountTapped);
            HelpCommand = new Command(OnHelpTapped);
        }

        public ICommand OpenWebCommand { get; }
        public ICommand AddModuleCommand { get; }
        public ICommand HelpCommand { get; }



        private SGModule _selectedItem;

        public ObservableCollection<SGModule> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command AccountCommand { get; }
        public Command<SGModule> ItemTapped { get; }

        async Task ExecuteLoadItemsCommand() {
            IsBusy = true;

            try {
                Items.Clear();
                var items = await ModulesDataStore.GetItemsAsync(true);
                foreach (var item in items) {
                    Items.Add(item);
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
            SelectedItem = null;
        }

        public SGModule SelectedItem {
            get => _selectedItem;
            set {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnAddItem(object obj) {
            await Shell.Current.GoToAsync(nameof(AddModulePage));
        }

        async void OnHelpTapped(object obj) {
            await Shell.Current.GoToAsync(nameof(HelpPage));
        }

        async void OnAccountTapped(object obj) {
            await Shell.Current.GoToAsync(nameof(AccountPage));
        }

        async void OnItemSelected(SGModule item) {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(SGModuleDetailPage)}?{nameof(SGModuleDetailViewModel.ItemId)}={item.Id}");
        }
    }
}
