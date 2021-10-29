using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;

namespace MobileApp.Common.Specifications.Managers {
    public interface IAPIManager : IDisposable {

        /// <summary>
        /// Performs a login and stores a Json Web Token (JwT) in the SettingsManager. 
        /// This token will be used by all following api requests/posts.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>True when login was successfully</returns>
        Task<bool> Login(string email, string password);

        /// <summary>
        /// Clears stored session information 
        /// </summary>
        void Logout();

        #region Module

        /// <summary>
        /// Requests all modules from rest api.
        /// </summary>
        /// <returns>Null when error</returns>
        Task<IEnumerable<ModuleInfoDto>> GetModules();

        Task<bool> UpdateModule(ModuleInfo updatedModule);

        Task<bool> AddModule(ModuleInfo newModule);

        Task<bool> DeleteModule(Guid moduleId);

        #endregion
    }
}
