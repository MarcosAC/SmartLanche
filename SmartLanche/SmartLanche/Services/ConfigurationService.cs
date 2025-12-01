using Microsoft.Extensions.Configuration;

namespace SmartLanche.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string IConfigurationService.GetLogoPath() => _configuration.GetSection("SmartLanche")["Name"] ?? "SmartLanche";
        string IConfigurationService.GetNameApp() => _configuration.GetSection("SmartLanche")["Logo"] ?? "SmartLanche";
        string IConfigurationService.GetTheme() => _configuration.GetSection("SmartLanche")["Theme"] ?? "SmartLanche";
    }
}
