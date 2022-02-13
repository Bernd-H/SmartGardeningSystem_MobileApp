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
        /// Checks if the stored aes keys are valid.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="keyValidationBytes">
        /// Aes encrypted byte array that contains a specific code and a salt.
        /// Will get decrypted by the basestation and checked if the decrypted data makes sense.
        /// </param>
        /// <returns>True when login was successfully</returns>
        /// <exception cref="MobileApp.Common.Exceptions.WrongAesKeyException">Gets thrown when the basestation could not decrypt the keyValidationBytes.</exception>
        Task<bool> Login(string email, string password, byte[] keyValidationBytes = null);

        /// <summary>
        /// Registers a new user.
        /// Does not request an token. Login must be called in order to being able to access fully the api.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>True when the registration was successful.</returns>
        //Task<bool> Register(string email, string password);

        Task<bool> ChangeLoginInfo(UpdateUserDto updateUserDto);

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
        Task<IEnumerable<ModuleInfo>> GetModules();

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> UpdateModule(ModuleInfo updatedModule);

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> AddModule(ModuleInfo newModule);

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> DeleteModule(byte moduleId);

        #endregion

        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> IsBasestationConnectedToWlan();

        Task<IEnumerable<WlanInfo>> GetWlans();

        Task<SystemStatus> GetSystemStatus();
    }
}
