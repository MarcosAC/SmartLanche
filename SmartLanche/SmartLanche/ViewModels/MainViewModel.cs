using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartLanche.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly ConfigurationService _configuration;
        public ProductRegistrationViewModel? ProductRegistrationViewModel { get; }

        public string AppName => _configuration.AppName;
        public string LogoPath => _configuration.LogoPath;

        public MainViewModel(ConfigurationService configuration, ProductRegistrationViewModel product)
        {
            _configuration = configuration;
            ProductRegistrationViewModel = product;
        }
    }
}
