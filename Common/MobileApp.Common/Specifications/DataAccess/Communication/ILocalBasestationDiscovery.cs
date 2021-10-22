using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface ILocalBasestationDiscovery : IDisposable {

        Task<BasestationFoundDto> TryFindBasestation();
    }
}
