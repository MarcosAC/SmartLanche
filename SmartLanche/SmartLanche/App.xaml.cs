using CommunityToolkit.Mvvm.Messaging;
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
                .AddJsonFile(Path.Combine("Config", "appsettings.json"), optional: false);

            Configuration = builder.Build();

            // Confgirações Banco de Dados
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
            
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(Configuration);

            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? Configuration["Database:ConnectionString"];

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);

            // Services e Repository
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            
            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<ProductRegistrationViewModel>();
            services.AddTransient<ClientRegistrationViewModel>();
            services.AddTransient<SalesViewModel>();
            services.AddTransient<PendingPaymentsViewModel>();

            // Views
            services.AddTransient<MainWindowView>();
            services.AddTransient<ProductRegistrationView>();
            services.AddTransient<ClientRegistrationView>();
            services.AddTransient<SalesView>();
            services.AddTransient<PendingPaymentsView>();

            ServiceProvider = services.BuildServiceProvider();

            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindowView>();
            mainWindow.Show();
        }
    }
}
