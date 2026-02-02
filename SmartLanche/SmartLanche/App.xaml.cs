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
                        
            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));


            // Services e Repository            
            services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();
            
            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ProductRegistrationViewModel>();
            services.AddSingleton<ClientRegistrationViewModel>();
            services.AddSingleton<SalesViewModel>();
            services.AddSingleton<PendingPaymentsViewModel>();
            services.AddSingleton<OrderStatusViewModel>();
            services.AddSingleton<InventoryViewModel>();

            // Views
            services.AddTransient<MainWindowView>();
            services.AddTransient<ProductRegistrationView>();
            services.AddTransient<ClientRegistrationView>();
            services.AddTransient<SalesView>();
            services.AddTransient<PendingPaymentsView>();
            services.AddTransient<OrderStatusView>();
            services.AddTransient<InventoryView>();

            ServiceProvider = services.BuildServiceProvider();

            var factory = ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using (var db = factory.CreateDbContext())
            {
                db.Database.EnsureCreated();
            }           

            var mainWindow = ServiceProvider.GetRequiredService<MainWindowView>();
            mainWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {            
            var culture = new System.Globalization.CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            base.OnStartup(e);
        }
    }
}
