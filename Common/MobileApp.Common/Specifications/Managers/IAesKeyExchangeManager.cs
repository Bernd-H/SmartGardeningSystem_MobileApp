using System.Threading;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {
    public interface IAesKeyExchangeManager {

        Task<bool> Start(CancellationToken token);
    }
}
