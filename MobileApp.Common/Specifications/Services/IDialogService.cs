using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Services {
    public interface IDialogService {
        Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback);
    }
}
