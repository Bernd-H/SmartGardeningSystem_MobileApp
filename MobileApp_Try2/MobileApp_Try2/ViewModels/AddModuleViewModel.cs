using System;
using System.Diagnostics;
using MobileApp_Try2.Views;
using Xamarin.Forms;

namespace MobileApp_Try2.ViewModels
{
    public class AddModuleViewModel : BaseViewModel
    {
        private string itemId;
        private string name;
        private string infoText;
        private string isOnline;
        private string measuredValue;
        private string id;

        public Command RemoveCommand { get; }

        public AddModuleViewModel() {
            Title = "Add new module";
        }

        public string Id 
        {
            get => id;
            set => SetProperty(ref id, value);
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
            }
        }

        public string ModuleImagePath {
            get { return "undraw_tabs"; }
        }
    }
}
