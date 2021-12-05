namespace MobileApp.Common.Specifications.Configuration {
    public interface IConnectionSettings {

        int LocalPeerDiscovery_port { get; set; }

        string LocalPeerDiscovery_multicastIP { get; set; }



        string API_URL_Login { get; set; }

        string API_URL_Register { get; set; }

        string API_URL_Modules { get; set; }

        string API_URL_Wlan { get; set; }

        int API_Port { get; set; }


        string ConfigurationWiFi_ServerIP { get; set; }

        string ConfigurationWiFi_KeyExchangeListenPort { get; set; }

        /// <summary>
        /// Port on which the command service is listening on the basestation
        /// </summary>
        int CommandsListener_Port { get; set; }


        string ExternalServer_Domain { get; set; }

        int ExternalServer_RelayPort { get; set; }
    }
}
