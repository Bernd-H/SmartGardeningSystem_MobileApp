using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp_Try2.Common.Specifications.Services {
    public interface IDialogService {
        Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback);
    }
}
