using System;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.Entities {
    public class ApplicationSettings : IDO {

        public Guid Id { get; set; }

        /// <summary>
        /// Exchanged to the mobile app securley. Used to decrypt
        /// packages from the CommandManager or relay packages received from the basestation over the external server.
        /// </summary>
        public byte[] AesKey { get; set; }

        /// <summary>
        /// Exchanged to the mobile app securley. Used to decrypt
        /// packages from the CommandManager or relay packages received from the basestation over the external server.
        /// </summary>
        public byte[] AesIV { get; set; }

        /// <summary>
        /// Certificate of the basestation
        /// </summary>
        public byte[] BasestationCert { get; set; }

        public string BaseStationIP { get; set; }

        public Guid BasestationId { get; set; }

        public Jwt SessionAPIToken { get; set; }
    }
}
