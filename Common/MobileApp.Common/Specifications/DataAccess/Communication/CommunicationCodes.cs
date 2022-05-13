namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public static class CommunicationCodes {

        public static byte[] ACK = new byte[] { 0xFF };

        public static byte[] WlanCommand = new byte[] { 150 };

        public static byte[] SendMoreInformationCommand = new byte[] { 148 };

        public static byte[] DisconnectFromWlanCommand = new byte[] { 149 };

        public static byte[] StartManualIrrigationCommand = new byte[] { 151 };

        public static byte[] StopManualIrrigationCommand = new byte[] { 152 };

        public static byte[] StartAutomaticIrrigationCommand = new byte[] { 153 };

        public static byte[] StopAutomaticIrrigationCommand = new byte[] { 154 };

        public static byte[] DiscoverNewModuleCommand = new byte[] { 155 };

        public static byte[] Test = new byte[] { 156 };

        public static byte[] PingModuleCommand = new byte[] { 157 };

        public static byte[] KeyValidationMessage = new byte[] { 0xAA, 0x55 };
    }
}
