using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GardeningSystem.Common.Models.DTOs;
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
        /// Registers a new user.
        /// Does not request an token. Login must be called in order to being able to access fully the api.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>True when the registration was successful.</returns>
        Task<bool> Register(string email, string password);

        /// <summary>
        /// Clears stored session information 
        /// </summary>
        void Logout();

        #region Module

        /// <summary>
        /// Requests all modules from rest api.
        /// </summary>
        /// <returns>Null when error</returns>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<IEnumerable<ModuleInfoDto>> GetModules();

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> UpdateModule(ModuleInfo updatedModule);

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> AddModule(ModuleInfo newModule);

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> DeleteModule(Guid moduleId);

        #endregion

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> IsBasestationConnectedToWlan();

        Task<IEnumerable<WlanInfo>> GetWlans();
    }
}
