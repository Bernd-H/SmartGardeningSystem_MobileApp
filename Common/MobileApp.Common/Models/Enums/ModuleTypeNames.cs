namespace MobileApp.Common.Models.Enums {
    public class ModuleTypeNames : AbstractEnum<string> {

        public static string SENSOR = "Sensor";
        
        public static string VALVE = "Valve";
        
        public static string MAINSTATION = "Mainstation";

        public ModuleTypeNames(string value) : base(value) {
        }
    }
}
