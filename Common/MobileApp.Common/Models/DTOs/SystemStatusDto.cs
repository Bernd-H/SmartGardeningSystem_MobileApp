namespace MobileApp.Common.Models.DTOs {

    public enum WateringStatus {
        Ready = 0,
        Watering = 1,
        StartingWatering = 11,
        StoppingWatering = 12,
        ManualWateringMode = 2,
        Error = 3
    }

    public class SystemStatusDto {

        public double SystemUpMinutes { get; set; }

        public float Temperature { get; set; }

        public WateringStatus WateringStatus { get; set; }
    }
}
