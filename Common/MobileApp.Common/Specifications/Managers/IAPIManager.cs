using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Common.Specifications.Managers {
    public interface IAPIManager {

        /// <summary>
        /// Performs a login and stores a Json Web Token (JwT) in the SettingsManager. 
        /// This token will be used by all following api requests/posts.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>True when login was successfully</returns>
        Task<bool> Login(string email, string password);
    }
}
