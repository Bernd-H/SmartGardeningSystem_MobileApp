using System;
using System.Collections.Generic;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class ModuleInfoDto : IDO {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ModuleTypes Type { get; set; }

        public IEnumerable<Guid> CorrespondingValves { get; set; }

        /// <summary>
        /// Time when the information got requested from the server
        /// </summary>
        public DateTime InformationTimestamp { get; set; }
    }
}
