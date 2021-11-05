namespace MobileApp.Common.Models.Entities {
    public class GlobalRuntimeVariables {

        public bool LoadedMainPageFirstTime { get; set; }

        public GlobalRuntimeVariables() {
            LoadedMainPageFirstTime = true;
        }
    }
}
