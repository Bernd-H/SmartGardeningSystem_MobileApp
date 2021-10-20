using System;
using System.Collections.Generic;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;
using Newtonsoft.Json;

namespace MobileApp.Common.Models.Entities {
    public class Jwt { // Json web token

        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
