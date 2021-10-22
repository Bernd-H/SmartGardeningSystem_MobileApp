using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Common.Models.Entities {
    public class ModuleInfo {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Guid> AssociatedModules { get; set; }

        public string ModuleTyp { get; set; }

        public IEnumerable<DateTime> LastWaterings { get; set; }
    }
}
