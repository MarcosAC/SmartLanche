using SmartLanche.Services;

namespace SmartLanche.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        public ProductRegistrationViewModel? ProductRegistrationViewModel { get; }

        public string AppName {  get; }
        public string LogoPath { get; }

        public MainViewModel(IConfigurationService configuration, ProductRegistrationViewModel product)
        {
            AppName = configuration.GetNameApp();
            LogoPath = configuration.GetLogoPath();

            ProductRegistrationViewModel = product;
        }
    }
}
