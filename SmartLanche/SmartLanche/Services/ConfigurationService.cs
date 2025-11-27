using Microsoft.Extensions.Configuration;

namespace SmartLanche.Services
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string AppName => _configuration.GetSection("SmartLanche")["Name"] ?? "SmartLanche";        
        public string LogoPath => _configuration.GetSection("SmartLanche")["Logo"] ?? "SmartLanche";
        public string Theme => _configuration.GetSection("SmartLanche")["Theme"] ?? "SmartLanche";
    }
}
