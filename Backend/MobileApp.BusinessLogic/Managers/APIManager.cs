using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Common.Specifications.Managers;

namespace MobileApp.BusinessLogic.Managers {
    public class APIManager : IAPIManager {

        public APIManager() {
        }

        public async Task<bool> Login(string email, string username) {
            return false;
        }
    }
}
