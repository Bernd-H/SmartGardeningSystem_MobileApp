using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MobileApp.Common.Models.DTOs;
using MobileApp.Common.Models.Entities;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Class that handles API requests to the basestation.
    /// </summary>
    public interface IAPIManager : IDisposable {

        /// <summary>
        /// Performs a login and stores a Json Web Token (JwT) in the SettingsManager. 
        /// This token will be used by all following api requests/posts.
        /// Checks if the stored aes key is valid.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Plaintext password.</param>
        /// <param name="keyValidationBytes">
        /// Aes encrypted byte array that contains a specific code and a salt.
        /// Will get decrypted by the basestation and checked if the decrypted data makes sense.
        /// </param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a boolean that is true when login was successfully.
        /// </returns>
        /// <exception cref="MobileApp.Common.Exceptions.WrongAesKeyException">Gets thrown when the basestation could not decrypt the keyValidationBytes.</exception>
        Task<bool> Login(string username, string password, byte[] keyValidationBytes = null);

        /// <summary>
        /// Changes the login information.
        /// </summary>
        /// <param name="updateUserDto">An object containing the current login information and the new login information.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a boolean that is true when the username and password got changed successfully.
        /// </returns>
        Task<bool> ChangeLoginInfo(UpdateUserDto updateUserDto);

        /// <summary>
        /// Clears stored session information.
        /// </summary>
        void Logout();

        #region Module

        /// <summary>
        /// Requests all modules which are connected with the basestation.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a list of all modules and is null when an error occured.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<IEnumerable<ModuleInfo>> GetModules();

        /// <summary>
        /// Updates a specific module.
        /// </summary>
        /// <param name="updatedModule">The updated module.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        /// <remarks>The ModuleId of the <paramref name="updatedModule"/> cannot be changed.</remarks>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> UpdateModule(ModuleInfo updatedModule);

        /// <summary>
        /// This method was just for test purposes and doesn't get supported by the basestation anymore.
        /// </summary>
        /// <param name="newModule">The module to add.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        [Obsolete]
        Task<bool> AddModule(ModuleInfo newModule);

        /// <summary>
        /// Deletes/Disconnects a module from the system.
        /// </summary>
        /// <param name="moduleId">Id of the module to remove.</param>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean indicating wether the operation was a success or not.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> DeleteModule(byte moduleId);

        #endregion

        /// <summary>
        /// Asks the basestation if it is connected to a wlan.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter is a boolean that is true when the basestation is connected to a wifi.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Gets thrown when token is not valid.</exception>
        Task<bool> IsBasestationConnectedToWlan();

        /// <summary>
        /// Requests the ssids of all wifis which the basestation can reach.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a list with all available wifis.
        /// </returns>
        Task<IEnumerable<WlanInfo>> GetWlans();

        /// <summary>
        /// Requests the system status of the basestation.
        /// </summary>
        /// <returns>
        /// A task that represents an asynchronous operation. The value of the TResult
        /// parameter contains a MobileApp.Common.Models.Entities.SystemStatus object.
        /// </returns>
        Task<SystemStatus> GetSystemStatus();
    }
}
