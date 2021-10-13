namespace MobileApp.DataAccess.Models {
    public class SGModule // Smart Gardening Module
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public SGModuleType _Type { get; set; }

        public string Type {
            get {
                if (_Type == SGModuleType.MAINSTATION)
                    return "Mainstation";
                else if (_Type == SGModuleType.SENSOR)
                    return "Sensor";
                else
                    return "Actor";
            }
        }

        public string InfoText { get; set; }

        public bool IsOnline { get; set; }

        public string MeasuredValue { get; set; }
    }

    public enum SGModuleType {
        SENSOR,
        ACTOR,
        MAINSTATION
    }
}
