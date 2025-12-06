using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using whiteboard_app.Services;
using whiteboard_app_data.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace whiteboard_app
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        public static ServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        /// <summary>
        /// Configures the dependency injection container with all required services.
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register DbContext with connection string
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WhiteboardApp",
                "whiteboard.db");

            var directory = System.IO.Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            services.AddDbContext<WhiteboardDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddScoped<IDataService, DataService>();
            services.AddSingleton<IDrawingService, DrawingService>();

            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Initialize database
            InitializeDatabase();

            _window = new MainWindow();
            _window.Activate();
        }

        /// <summary>
        /// Initializes the database by applying migrations and seeding default data.
        /// </summary>
        private void InitializeDatabase()
        {
            if (ServiceProvider == null)
                return;

            try
            {
                using var scope = ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<WhiteboardDbContext>();
                
                // Ensure database is created
                if (!context.Database.CanConnect())
                {
                    // Apply pending migrations
                    context.Database.Migrate();
                }
                else
                {
                    // Database exists, check if migrations are pending
                    var pendingMigrations = context.Database.GetPendingMigrations();
                    if (pendingMigrations.Any())
                    {
                        context.Database.Migrate();
                    }
                }
                
                // Seed initial data (force seed if empty)
                DbInitializer.Initialize(context);
                
                // Double-check: if still no profiles, force seed
                var finalProfileCount = context.Profiles.Count();
                if (finalProfileCount == 0)
                {
                    DbInitializer.ForceSeed(context);
                }
                
                // Force refresh
                context.SaveChanges();
            }
            catch (Exception)
            {
                // Try to continue - database might already exist
            }
        }
    }
}
