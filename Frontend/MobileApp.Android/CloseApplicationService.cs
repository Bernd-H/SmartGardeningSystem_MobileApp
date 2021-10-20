using Android.App;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.Droid.Close {
    public class CloseApplicationService : ICloseApplicationService {
        public void CloseApplication() {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();
        }
    }
}
