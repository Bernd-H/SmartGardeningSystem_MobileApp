using System;
using System.Collections.Generic;

namespace MobileApp.Common.Models.DTOs {
    public class ModuleInfoDto {

        public string ModuleId { get; set; }

        public string Name { get; set; }

        public string ModuleTypeName { get; set; }

        public IEnumerable<byte> AssociatedModules { get; set; }

        public IEnumerable<DateTime> LastWaterings { get; set; }

        public bool EnabledForManualIrrigation { get; set; }

        /// <summary>
        /// Time when the information got requested from the server
        /// </summary>
        public DateTime InformationTimestamp { get; set; }
    }
}
