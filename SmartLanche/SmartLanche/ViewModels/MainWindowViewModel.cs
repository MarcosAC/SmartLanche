using SmartLanche.Services;

namespace SmartLanche.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ProductRegistrationViewModel? ProductRegistrationViewModel { get; }

        public string AppName {  get; }
        public string LogoPath { get; }

        public MainWindowViewModel(IConfigurationService configuration, ProductRegistrationViewModel product)
        {
            AppName = configuration.GetNameApp();
            LogoPath = configuration.GetLogoPath();

            ProductRegistrationViewModel = product;
        }
    }
}
