using System;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class WlanInfoDto : IDO {
        public Guid Id { get; set; }

        public string Ssid { get; set; }

        public byte[] EncryptedPassword { get; set; }
    }
}
