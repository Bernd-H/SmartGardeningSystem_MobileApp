using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Models.Entities;
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

        private bool activityIndicatorIsVisible = true;
        public bool ActivityIndicatorIsVisible {
            get => activityIndicatorIsVisible;
            set => SetProperty(ref activityIndicatorIsVisible, value);
        }

        public ICommand ViewLogsPageCommand { get; }

        public ICommand AbortCommand { get; }


        private ILogger Logger;

        private IDialogService DialogService;

        private ICommandManager CommandManager;

        private IDataStore<ModuleInfo> ModuleRepository;

        private CancellationTokenSource cancellationTS = new CancellationTokenSource();

        public WaitingForNewModulePageViewModel(ILoggerService loggerService, IDialogService dialogService, ICommandManager commandManager,
            IDataStore<ModuleInfo> moduleRepository) {
            Logger = loggerService.GetLogger<WaitingForNewModulePageViewModel>();
            DialogService = dialogService;
            CommandManager = commandManager;
            ModuleRepository = moduleRepository;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);
            AbortCommand = new Command(OnAbortTapped);

            _ = BeginSearch();
        }

        ~WaitingForNewModulePageViewModel() {
            cancellationTS.Cancel();
        }

        async Task BeginSearch() {
            ActivityIndicatorIsVisible = true;
            byte? moduleId = await CommandManager.DiscoverNewModule(cancellationTS.Token);

            if (!moduleId.HasValue) {
                ActivityIndicatorIsVisible = false;
                await DialogService.ShowMessage("No new module found!", "Info", "Ok", null);
                await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
            }
            else if (!cancellationTS.IsCancellationRequested) {
                Logger.Info($"[BeginSearch]Found new module with id={Utils.ConvertByteToHex(moduleId.Value)}.");

                // request all modules from the api of the basestation to update the internal module cache
                await ModuleRepository.GetItemsAsync(true);

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
            await Shell.Current.GoToAsync(PageNames.GetNavigationString(PageNames.MainPage));
        }
    }
}
