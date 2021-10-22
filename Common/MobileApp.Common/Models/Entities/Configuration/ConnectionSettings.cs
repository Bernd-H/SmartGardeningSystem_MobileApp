using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Models.Entities.Configuration {
    public class ConnectionSettings : IConnectionSettings {
        public int LocalPeerDiscovery_port { get; set; }

        public string LocalPeerDiscovery_multicastIP { get; set; }


        public string API_URL_Login { get; set; }

        public string API_URL_Modules { get; set; }

        public int API_Port { get; set; }


        public string ConfigurationWiFi_ServerIP { get; set; }

        public string ConfigurationWiFi_KeyExchangeListenPort { get; set; }
    }
}
