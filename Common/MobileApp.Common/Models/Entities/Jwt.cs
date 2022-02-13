using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;
using Newtonsoft.Json;

namespace MobileApp.Common.Models.Entities {
    public class Jwt { // Json web token

        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        public bool IsTokenValid() {
            if (!string.IsNullOrWhiteSpace(Token)) {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(Token);
                return (jwtSecurityToken.ValidTo - DateTime.UtcNow) > TimeSpan.Zero;
            }

            return false;
        }
    }
}
