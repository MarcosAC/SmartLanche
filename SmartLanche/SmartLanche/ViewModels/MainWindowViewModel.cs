using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SmartLanche.Services;

namespace SmartLanche.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ProductRegistrationViewModel ProductRegistrationViewModel { get; }
        public ClientRegistrationViewModel ClientRegistrationViewModel { get; }

        private readonly IMessenger _messenger;       

        public string AppName {  get; }
        public string LogoPath { get; }        

        public MainWindowViewModel(
            IConfigurationService configuration,
            ProductRegistrationViewModel productRegistrationViewModel,
            ClientRegistrationViewModel clientRegistrationViewModel,
            IMessenger messenger)
        {
            AppName = configuration.GetNameApp();
            LogoPath = configuration.GetLogoPath();

            ProductRegistrationViewModel = productRegistrationViewModel;
            ClientRegistrationViewModel = clientRegistrationViewModel;

            _messenger = messenger;
        }        
    }
}
