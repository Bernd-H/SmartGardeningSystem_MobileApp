namespace MobileApp.Common.Specifications.Configuration {
    public interface IConnectionSettings {

        int LocalPeerDiscovery_port { get; set; }

        string LocalPeerDiscovery_multicastIP { get; set; }



        string API_URL_Login { get; set; }

        int API_Port { get; set; }


        string ConfigurationWiFi_ServerIP { get; set; }

        string ConfigurationWiFi_KeyExchangeListenPort { get; set; }
    }
}
