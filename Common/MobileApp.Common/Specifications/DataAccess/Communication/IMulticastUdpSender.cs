using System;
using System.Net;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IMulticastUdpSender : IDisposable {

        Task SendToMulticastGroupAsync(IPAddress localIP, int replyPort);
    }
}
