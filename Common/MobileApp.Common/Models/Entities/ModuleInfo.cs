using System;
using System.Collections.Generic;
using MobileApp.Common.Models.Enums;

namespace MobileApp.Common.Models.Entities {
    public class ModuleInfo {

        public byte ModuleId { get; set; }

        public string Name { get; set; }

        public ModuleType ModuleType { get; set; }

        public IEnumerable<byte> AssociatedModules { get; set; }

        public IEnumerable<DateTime> LastWaterings { get; set; }

        public bool EnabledForManualIrrigation { get; set; }

        /// <summary>
        /// Time when the information got requested from the server
        /// </summary>
        public DateTime InformationTimestamp { get; set; }
    }
}
