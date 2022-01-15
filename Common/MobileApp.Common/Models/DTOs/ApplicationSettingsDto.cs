using System;
using System.Security.Cryptography.X509Certificates;
using MobileApp.Common.Models.Entities;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class ApplicationSettingsDto : IDO {

        public Guid Id { get; set; }

        /// <summary>
        /// Exchanged to the mobile app securley. Used to decrypt
        /// the authentication information (in RestAPI) sent by the mobile app.
        /// </summary>
        public byte[] AesKey { get; set; }

        /// <summary>
        /// Exchanged to the mobile app securley. Used to decrypt
        /// the authentication information (in RestAPI) sent by the mobile app.
        /// </summary>
        public byte[] AesIV { get; set; }

        /// <summary>
        /// Certificate of the basestation
        /// </summary>
        public X509Certificate BasestationCert { get; set; }

        public string BaseStationIP { get; set; }

        public Guid BasestationId { get; set; }

        public Jwt SessionAPIToken { get; set; }

        public static ApplicationSettingsDto GetStandardSettings() {
            return new ApplicationSettingsDto() {
                Id = Guid.NewGuid(),
                BasestationId = Guid.Empty,
                BaseStationIP = string.Empty,
                //BaseStationIP = "10.0.2.2",
                //BaseStationIP = "192.168.1.48",
                SessionAPIToken = null
            };
        }
    }
}
