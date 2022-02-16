using System;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class that manages the application settings.
    /// </summary>
    public interface ISettingsManager {

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a MobileApp.Common.Models.DTOs.ApplicationSettingsDto object.
        /// </returns>
        Task<ApplicationSettingsDto> GetApplicationSettings();

        /// <summary>
        /// Ensures that the current settings get passed to the <paramref name="updateFunc"/> function.
        /// This method is thread safe.
        /// </summary>
        /// <param name="updateFunc">Function that takes the current settings and returns the updated settings.</param>
        /// <returns>A task that represents an asynchronous operation.</returns>
        Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc);

        /// <summary>
        /// Gets the global runtime settings.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a MobileApp.Common.Models.Entities.GlobalRuntimeVariables object.
        /// </returns>
        GlobalRuntimeVariables GetRuntimeVariables();

        /// <summary>
        /// Updates the global runtime settings.
        /// Ensures that the current settings get passed to the <paramref name="updateFunc"/> function.
        /// </summary>
        /// <param name="updateFunc">Function that takes the current global runtime settings and that returns the updated settings.</param>
        void UpdateCurrentRuntimeVariables(Func<GlobalRuntimeVariables, GlobalRuntimeVariables> updateFunc);
    }
}
