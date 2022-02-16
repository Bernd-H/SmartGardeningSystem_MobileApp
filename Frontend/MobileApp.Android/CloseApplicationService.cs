using Android.App;
using MobileApp.Common.Specifications.Services;
using Xamarin.Forms;

namespace MobileApp.Droid.Close {

    /// <inheritdoc/>
    public class CloseApplicationService : ICloseApplicationService {

        /// <inheritdoc/>
        public void CloseApplication() {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();
        }
    }
}
