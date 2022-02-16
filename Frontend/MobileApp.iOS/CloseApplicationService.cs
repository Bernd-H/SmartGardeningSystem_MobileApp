using System.Threading;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.iOS {

    /// <inheritdoc/>
    public class CloseApplicationService : ICloseApplicationService {

        /// <inheritdoc/>
        public void CloseApplication() {
            Thread.CurrentThread.Abort();
        }
    }
}
