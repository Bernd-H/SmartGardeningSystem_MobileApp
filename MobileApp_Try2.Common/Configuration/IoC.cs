using Xamarin.Forms;

namespace MobileApp_Try2.Common.Configuration {
    public static class IoC {
        public static T Get<T>() where T : class {
            return DependencyService.Get<T>();
        }
    }
}
