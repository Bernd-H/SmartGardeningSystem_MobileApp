using System;
using System.Collections.Generic;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class UserDto { 

        public string Username { get; set; }

        public string Password { get; set; }

        public byte[] KeyValidationBytes { get; set; }
    }
}
