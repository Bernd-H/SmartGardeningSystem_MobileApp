using System;

namespace MobileApp.Common.Models.Entities {
    public class ValueTimePair<T> {

        public DateTime Timestamp { get; set; }

        public T Value { get; set; }
    }
}
