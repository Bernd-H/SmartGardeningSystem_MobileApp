using System;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.Entities {
    public class ServicePackage : IServicePackage {

        public Guid SessionId { get; set; }

        public byte[] Data { get; set; }
    }
}
