using System.Threading;
using MobileApp.Common.Specifications.Services;

namespace MobileApp.iOS {
    public class CloseApplicationService : ICloseApplicationService {
        public void CloseApplication() {
            Thread.CurrentThread.Abort();
        }
    }
}
