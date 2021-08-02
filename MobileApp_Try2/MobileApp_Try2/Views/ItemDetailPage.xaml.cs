using MobileApp_Try2.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace MobileApp_Try2.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}