
using Foundation;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Services;
using MobileApp.DataAccess;
using MobileApp.iOS.Renders;
using TinyIoC;
using UIKit;
using Xamarin.Forms;

namespace MobileApp.iOS {
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
            RegisterPlatformSpecificDependencies();

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void RegisterPlatformSpecificDependencies() {
            var container = TinyIoCContainer.Current;

            //DependencyService.Register<ITextWidth, CalculateTextWidthiOS>();
            container.Register<ITextWidth, CalculateTextWidthiOS>().AsSingleton();

            //DependencyService.Register<IFileStorage, FileStorageiOS>();
            container.Register<IFileStorage, FileStorageiOS>().AsMultiInstance();

            container.Register<ICloseApplicationService, CloseApplicationService>();
        }
    }
}
