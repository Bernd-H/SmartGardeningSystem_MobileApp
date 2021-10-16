using System;
using System.Collections.Generic;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class UserDto : IDO {
        public Guid Id { get; set; }

        public byte[] AesEncryptedEmail { get; set; }

        public byte[] AesEncryptedPassword { get; set; }
    }
}
