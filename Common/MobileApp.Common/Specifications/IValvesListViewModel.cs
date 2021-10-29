using System.Collections.ObjectModel;
using MobileApp.Common.Models.DTOs;

namespace MobileApp.Common.Specifications {
    public interface IValvesListViewModel {
        /// <summary>
        /// List of valves which gets displayed in an view model
        /// </summary>
        ObservableCollection<ModuleInfoDto> LinkedValves { get; set; }
    }
}
