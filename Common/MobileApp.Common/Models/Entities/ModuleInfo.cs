using System;
using System.Collections.Generic;
using MobileApp.Common.Models.Enums;

namespace MobileApp.Common.Models.Entities {
    public class ModuleInfo {

        public byte ModuleId { get; set; }

        public string Name { get; set; }

        public ModuleType ModuleType { get; set; }

        /// <summary>
        /// Time when the information got requested from the server
        /// </summary>
        public DateTime InformationTimestamp { get; set; }

        /// <summary>
        /// Last measured signal strength to the module.
        /// </summary>
        public ValueTimePair<int> SignalStrength { get; set; }

        /// <summary>
        /// Last measured battery level of the module.
        /// </summary>
        public ValueTimePair<float> BatteryLevel { get; set; }

        /// <summary>
        /// List of temperature measurements of the module.
        /// </summary>
        public IList<ValueTimePair<float>> TemperatureMeasurements { get; set; }

        #region Valve properties

        /// <summary>
        /// Property for valves.
        /// Sensors that are associated to this valve.
        /// </summary>
        public IList<byte> AssociatedModules { get; set; }

        /// <summary>
        /// Property for valves.
        /// List of irrigation DateTimes with the time the valve was open in minutes.
        /// </summary>
        public IList<ValueTimePair<int>> LastWaterings { get; set; }

        /// <summary>
        /// Property for valves.
        /// True to open or close this valve when the system gets controlled manually.
        /// </summary>
        public bool EnabledForManualIrrigation { get; set; }

        #endregion

        #region Sensor properties

        /// <summary>
        /// Property for sensors.
        /// List of soil moisture measurements.
        /// </summary>
        public IList<ValueTimePair<float>> SoilMoistureMeasurements { get; set; }

        #endregion
    }
}
