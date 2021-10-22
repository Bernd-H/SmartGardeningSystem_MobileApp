using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.DataAccess.Communication {
    public interface IMulticastUdpSender : IDisposable {

        Task SendToMulticastGroupAsync(int replyPort);
    }
}
