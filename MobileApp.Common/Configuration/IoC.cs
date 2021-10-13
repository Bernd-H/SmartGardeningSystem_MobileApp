using Xamarin.Forms;

namespace MobileApp.Common.Configuration {
    public static class IoC {
        public static T Get<T>() where T : class {
            return DependencyService.Get<T>();
            //return DependencyService.Resolve<T>();
        }
    }
}
