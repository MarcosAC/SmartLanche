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
                "Inventory" => _serviceProvider.GetRequiredService<InventoryViewModel>(),
                _ => CurrentViewModel
            };
        }
    }
}
