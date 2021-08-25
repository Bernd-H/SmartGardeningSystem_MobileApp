using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace MobileApp_Try2.BusinessLogic.ViewModels {
    public class AddModuleViewModel : BaseViewModel
    {
        private string name;
        private string id;

        public Command AddAndConnectCommand { get; }

        public AddModuleViewModel() {
            Title = "Add new module";
            AddAndConnectCommand = new Command(OnAddAndConnect);
        }

        public string Id 
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string AddModuleImagePath {
            get { return "undraw_Mind_map_re_nlb6"; }
        }

        async void OnAddAndConnect(object obj) {

        }
    }
}
