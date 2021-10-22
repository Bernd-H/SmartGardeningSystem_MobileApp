using System;
using System.Collections.Generic;
using System.Text;
using MobileApp.Common.Specifications.DataObjects;

namespace MobileApp.Common.Models.DTOs {

    public class ModuleType {
        public static string SENSOR = "Sensor";
        public static string ACTOR = "Actor";
        public static string MAINSTATION = "Mainstation";

        public string Value { get; private set; }

        public ModuleType(string value) {
            // check for invalid value
            if (value != SENSOR && value != ACTOR && value != MAINSTATION)
                throw new ArgumentException();

            Value = value;
        }
    }

    public class ModuleInfoDto : IDO {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ModuleType Type { get; set; }

        public string InfoText { get; set; }

        public bool IsOnline { get; set; }

        public string MeasuredValue { get; set; }
    }
}
