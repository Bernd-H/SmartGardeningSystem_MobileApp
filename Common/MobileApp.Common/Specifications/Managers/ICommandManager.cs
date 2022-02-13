using System;
using System.Threading;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;

namespace MobileApp.Common.Specifications.Managers {
    public interface ICommandManager : IDisposable {

        /// <param name="wlanInfo">wlan login data</param>
        /// <returns>If the operation was successfull</returns>
        Task<bool> ConnectToWlan(WlanInfoDto wlanInfo);

        Task<bool> DisconnectFromWlan();

        Task<bool> StartAutomaticIrrigation();

        Task<bool> StopAutomaticIrrigation();

        Task<bool> StartManualIrrigation(TimeSpan timeSpan);

        Task<bool> StopManualIrrigation();

        Task<byte?> DiscoverNewModule(CancellationToken cancellationToken);

        Task<bool> Test();
    }
}
