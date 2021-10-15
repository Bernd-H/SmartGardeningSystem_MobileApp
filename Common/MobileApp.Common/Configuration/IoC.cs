using Xamarin.Forms;

namespace MobileApp.Common.Configuration {
    public static class IoC {
        public static T Get<T>() where T : class {
            //return DependencyService.Get<T>(); // DependencyService is no dependency injection framework, its a service locator framework
            return TinyIoC.TinyIoCContainer.Current.Resolve<T>();
        }
    }
}
