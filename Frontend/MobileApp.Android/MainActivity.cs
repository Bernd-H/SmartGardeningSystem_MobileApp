
using System;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using MobileApp.Common.Configuration;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.DataAccess;
using MobileApp.Common.Specifications.Services;
using MobileApp.DataAccess;
using MobileApp.Droid.Close;
using MobileApp.Droid.Renders;
using TinyIoC;
using Xamarin.Forms;

namespace MobileApp.Droid {
    [Activity(Label = "MobileApp", Icon = "@mipmap/appicon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            RegisterPlatformSpecificDependencies();

            base.OnCreate(savedInstanceState);

            try {
                // load config file
                using (var inputStream = Assets.Open(ConfigurationStore.ConfigFileName)) {
                    using (var streamReader = new StreamReader(inputStream)) {
                        ConfigurationStore.ConfigurationContent = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception) {
                ConfigurationStore.SetConfigToStandardValues();
            }

            // load private key for sftp access (for debug purposes)
            //Stream sftpKeyStream = Assets.Open("rsa_piKeys.ppk");
            Stream sftpKeyStream = null;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(sftpKeyStream));
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RegisterPlatformSpecificDependencies() {
            var container = TinyIoCContainer.Current;

            //DependencyService.Register<ITextWidth, CalculateTextWidthAndroid>();
            container.Register<ITextWidth, CalculateTextWidthAndroid>().AsSingleton();

            //DependencyService.Register<IFileStorage, FileStorageAndroid>();
            container.Register<IFileStorage, FileStorageAndroid>().AsMultiInstance();

            container.Register<ICloseApplicationService, CloseApplicationService>();
        }
    }
}
