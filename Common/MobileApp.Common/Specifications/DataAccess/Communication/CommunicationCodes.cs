namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public static class CommunicationCodes {

        public static byte[] ACK = new byte[] { 200, 3, 184, 45, 234, 13, 147, 122 };

        public static byte[] WlanCommand = new byte[] { 150 };

        public static byte[] DisconnectFromWlanCommand = new byte[] { 149 };

        public static byte[] StartManualIrrigationCommand = new byte[] { 151 };

        public static byte[] StopManualIrrigationCommand = new byte[] { 152 };

        public static byte[] StartAutomaticIrrigationCommand = new byte[] { 153 };

        public static byte[] StopAutomaticIrrigationCommand = new byte[] { 154 };
    }
}
