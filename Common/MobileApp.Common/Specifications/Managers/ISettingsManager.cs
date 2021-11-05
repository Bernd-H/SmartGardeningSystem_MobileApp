using System;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;

namespace MobileApp.Common.Specifications.Managers {
    public interface ISettingsManager {

        Task<ApplicationSettingsDto> GetApplicationSettings();

        /// <summary>
        /// Ensures that up to date settings get passed to updateFunc and
        /// multiple threads can not change settings while calling this function.
        /// </summary>
        /// <param name="updateFunc">gets current settings and must return the changed settings</param>
        Task UpdateCurrentSettings(Func<ApplicationSettingsDto, ApplicationSettingsDto> updateFunc);


        GlobalRuntimeVariables GetRuntimeVariables();

        void UpdateCurrentRuntimeVariables(Func<GlobalRuntimeVariables, GlobalRuntimeVariables> updateFunc);
    }
}
