namespace MobileApp.Common.Models.Enums {
    public class WateringMethods : AbstractEnum<string> {

        public static string DROPBYDROP = "drop-by-drop system";
        public static string SPRINKLER = "sprinkler";

        public WateringMethods(string value) : base(value) {
        }
    }
}
