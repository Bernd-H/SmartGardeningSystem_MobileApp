using System;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;

namespace MobileApp.Common.Specifications.Managers {
    public interface ICommandManager : IDisposable {

        /// <param name="wlanInfo">wlan login data</param>
        /// <returns>If the operation was successfull</returns>
        Task<bool> ConnectToWlan(WlanInfoDto wlanInfo);
    }
}
