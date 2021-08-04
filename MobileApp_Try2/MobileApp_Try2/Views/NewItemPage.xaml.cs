using MobileApp_Try2.Models;
using MobileApp_Try2.ViewModels;
using Xamarin.Forms;

namespace MobileApp_Try2.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}