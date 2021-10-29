using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MobileApp.Common;
using MobileApp.Common.Specifications;
using MobileApp.Common.Specifications.Managers;
using MobileApp.Common.Specifications.Services;
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

        private IAPIManager APIManager;

        private CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public WaitingForNewModulePageViewModel(ILoggerService loggerService, IDialogService dialogService, IAPIManager _APIManager) {
            Logger = loggerService.GetLogger<WaitingForNewModulePageViewModel>();
            DialogService = dialogService;
            APIManager = _APIManager;

            ViewLogsPageCommand = new Command(OnViewLogsTapped);
            AbortCommand = new Command(OnAbortTapped);

            _ = BeginSearch();
        }

        ~WaitingForNewModulePageViewModel() {
            cancellationToken.Cancel();
        }

        async Task BeginSearch() {
            await Task.Delay(1000);
            bool addingASensor = true;
            await Shell.Current.GoToAsync($"{PageNames.AddModulePage}?{nameof(AddModuleViewModel.AddingASensor)}={addingASensor}");
        }

        async void OnViewLogsTapped(object obj) {
            await Shell.Current.GoToAsync(PageNames.LogsPage);
        }

        async void OnAbortTapped(object obj) {
            // abort searching


            // navigate back to home page
            await Shell.Current.GoToAsync(PageNames.MainPage);
        }
    }
}
