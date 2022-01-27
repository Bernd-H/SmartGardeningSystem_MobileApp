using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
using MobileApp.Common.Utilities;
using NLog;
using Xamarin.Forms;

namespace MobileApp.BusinessLogic.ViewModels {
    public class WaitingForNewModulePageViewModel : BaseViewModel {

        private string status = "-";
        public string Status {
            get => status;
            set => SetProperty(ref status, value);
        }

        public ICommand ViewLogsPageCommand { get; }

        public ICommand AbortCommand { get; }


        private ILogger Logger;

        private IDialogService DialogService;

        private ICommandManager CommandManager;

        private CancellationTokenSource cancellationTS = new CancellationTokenSource();

        public WaitingForNewModulePageViewModel(ILoggerService loggerService, IDialogService dialogService, ICommandManager commandManager) {
            Logger = loggerService.GetLogger<WaitingForNewModulePageViewModel>();
            DialogService = dialogService;
            CommandManager = commandManager;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);
            AbortCommand = new Command(OnAbortTapped);

            _ = BeginSearch();
        }

        ~WaitingForNewModulePageViewModel() {
            cancellationTS.Cancel();
        }

        async Task BeginSearch() {
            byte? moduleId = await CommandManager.DiscoverNewModule(cancellationTS.Token);

            if (!moduleId.HasValue) {
                await DialogService.ShowMessage("Found no new module.", "Info", "Ok", null);
            }
            else if (!cancellationTS.IsCancellationRequested) {
                await Shell.Current.GoToAsync($"{PageNames.AddModulePage}?{nameof(AddModuleViewModel.ModuleId)}={Utils.ConvertByteToHex(moduleId.Value)}");
            }
        }

        async void OnViewLogsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }

        async void OnAbortTapped(object obj) {
            // abort searching
            cancellationTS.Cancel();

            // navigate back to home page
            await Shell.Current.GoToAsync(PageNames.MainPage);
        }
    }
}
