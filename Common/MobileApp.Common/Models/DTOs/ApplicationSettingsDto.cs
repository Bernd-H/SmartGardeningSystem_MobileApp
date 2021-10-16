using System;
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


        public string BaseStationIP { get; set; }

        public static ApplicationSettingsDto GetStandardSettings() {
            return new ApplicationSettingsDto() {
                Id = Guid.NewGuid(),
                BaseStationIP = "localhost"
            };
        }
    }
}
