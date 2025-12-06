using Microsoft.Extensions.Configuration;
using System.IO;

namespace SmartLanche.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetLogoPath()
        {
            string relativePath = _configuration.GetSection("SmartLanche")["Logo"] ?? "Resource/logo.jpg";
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            return fullPath;
        }

        public string GetNameApp() => _configuration.GetSection("SmartLanche")["Name"] ?? "SmartLanche";

        public string GetTheme() => _configuration.GetSection("SmartLanche")["Theme"] ?? "Orange";
    }
}
