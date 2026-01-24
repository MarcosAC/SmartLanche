using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SmartLanche.Services;

namespace SmartLanche.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private readonly IServiceProvider _serviceProvider;
       
        [ObservableProperty]
        private object? _currentViewModel;

        public string AppName { get; }
        public string LogoPath { get; }

        public MainWindowViewModel(
            IConfigurationService configuration,
            IServiceProvider serviceProvider,
            IMessenger messenger) : base(messenger)
        {
            _serviceProvider = serviceProvider;
            AppName = configuration.GetNameApp();
            LogoPath = configuration.GetLogoPath();
            
            Navigate("Sales");
        }

        [RelayCommand]
        public void Navigate(string destination)
        {
            CurrentViewModel = destination switch
            {
                "Sales" => _serviceProvider.GetRequiredService<SalesViewModel>(),
                "Products" => _serviceProvider.GetRequiredService<ProductRegistrationViewModel>(),
                "Clients" => _serviceProvider.GetRequiredService<ClientRegistrationViewModel>(),
                "Status" => _serviceProvider.GetRequiredService<OrderStatusViewModel>(),
                "Payments" => _serviceProvider.GetRequiredService<PendingPaymentsViewModel>(),
                _ => CurrentViewModel
            };
        }
    }
}




//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Messaging;
//using SmartLanche.Services;

//namespace SmartLanche.ViewModels
//{
//    public partial class MainWindowViewModel : ObservableObject
//    {
//        public ProductRegistrationViewModel ProductRegistrationViewModel { get; }
//        public ClientRegistrationViewModel ClientRegistrationViewModel { get; }
//        public SalesViewModel SalesViewModel { get; }
//        public PendingPaymentsViewModel PendingPaymentsViewModel { get; }
//        public OrderStatusViewModel OrderStatusViewModel { get; }

//        private readonly IMessenger _messenger;       

//        public string AppName {  get; }
//        public string LogoPath { get; }        

//        public MainWindowViewModel(
//            IConfigurationService configuration,
//            ProductRegistrationViewModel productRegistrationViewModel,
//            ClientRegistrationViewModel clientRegistrationViewModel,
//            SalesViewModel salesViewModel,
//            PendingPaymentsViewModel pendingPaymentsViewModel,
//            OrderStatusViewModel orderStatusViewModel,
//            IMessenger messenger)
//        {
//            AppName = configuration.GetNameApp();
//            LogoPath = configuration.GetLogoPath();

//            ProductRegistrationViewModel = productRegistrationViewModel;
//            ClientRegistrationViewModel = clientRegistrationViewModel;
//            SalesViewModel = salesViewModel;
//            PendingPaymentsViewModel = pendingPaymentsViewModel;
//            OrderStatusViewModel = orderStatusViewModel;

//            _messenger = messenger;
//        }        
//    }
//}
