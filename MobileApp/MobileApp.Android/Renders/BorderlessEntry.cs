// Class to remove the line by entry elements on the android platform
using Android.Content;
using MobileApp.Droid.Renders;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(BorderlessEntry))]
namespace MobileApp.Droid.Renders
{
    public class BorderlessEntry : EntryRenderer
    {
        public BorderlessEntry(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Background = null;
            }
        }
    }
}
