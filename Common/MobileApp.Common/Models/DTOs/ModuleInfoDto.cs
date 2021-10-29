using System;
using System.Collections.Generic;
using MobileApp.Common.Models.Enums;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {
    public class ModuleInfoDto : IDO {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ModuleTypes Type { get; set; }

        public string InfoText { get; set; }

        public bool IsOnline { get; set; }

        public string MeasuredValue { get; set; }

        public IEnumerable<Guid> CorrespondingValves { get; set; }
    }
}
