using System;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Services {

    /// <summary>
    /// Class to show messages to the user.
    /// </summary>
    public interface IDialogService {

        /// <summary>
        /// Displays a dialog to the user.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="title">Title.</param>
        /// <param name="buttonText">The text of the button. (for example: "Close")</param>
        /// <param name="afterHideCallback">Method that gets called after the user pressed the button with the text=<paramref name="buttonText"/>.</param>
        /// <returns>A Task that reprecents an asynchronous operation.</returns>
        Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback);
    }
}
