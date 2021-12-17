using System;

namespace MobileApp.Common.Exceptions {
    public class ConnectionClosedException : Exception {

        public Guid NetworkStreamId { get; set; }

        public ConnectionClosedException() {

        }

        public ConnectionClosedException(string message) : base(message) {
        }

        public ConnectionClosedException(string message, Exception inner)
            : base(message, inner) {
        }

        public ConnectionClosedException(Guid networkStreamId) {
            NetworkStreamId = networkStreamId;
        }
    }
}
