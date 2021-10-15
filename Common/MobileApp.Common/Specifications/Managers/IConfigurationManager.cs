using System.Threading.Tasks;
using MobileApp.Common.Specifications.Configuration;

namespace MobileApp.Common.Specifications.Managers {

    /// <summary>
    /// Gets registered as singleton in the dependency framework
    /// </summary>
    public interface IConfigurationManager {

        Task<IConfiguration> GetConfig();
    }
}
