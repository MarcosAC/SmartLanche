using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartLanche.Data;
using SmartLanche.Services;
using SmartLanche.ViewModels;
using SmartLanche.Views;
using System.IO;
using System.Windows;

namespace SmartLanche
{  
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public static IConfiguration? Configuration { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs eventArgs)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(Path.Combine("Config", "appsettings.json"), optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(Configuration);

            var connection = Configuration.GetSection("Database")["ConnectionString"]
                             ?? @"Server=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Data\SmartLanche.mdf;Integrated Security=True;";

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ProductRegistrationViewModel>();

            services.AddTransient<MainWindowView>();
            services.AddTransient<ProductRegistrationView>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindowView>();
            mainWindow?.Show();
        }
    }
}
