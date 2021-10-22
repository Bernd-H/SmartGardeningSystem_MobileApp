using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class BasestationFoundDto : IDO {
        public Guid Id { get; set; }

        public IPEndPoint RemoteEndPoint { get; set; }
    }
}
