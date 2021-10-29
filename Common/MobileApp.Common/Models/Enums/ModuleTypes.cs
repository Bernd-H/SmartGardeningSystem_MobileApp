namespace MobileApp.Common.Models.Enums {
    public class ModuleTypes : AbstractEnum<string> {
        public static string SENSOR = "Sensor";
        public static string VALVE = "Actor";
        public static string MAINSTATION = "Mainstation";

        public ModuleTypes(string value) : base(value) {
        }
    }
}
