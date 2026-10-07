using AgroEco.Core;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs;
using AgroEco.Data;
using AgroEco.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace AgroEco.UI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;
        

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
         

            ServiceProvider = DependencyConfigurator.ConfigureServices();
            var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            var jobEngine = ServiceProvider.GetRequiredService<JobEngine>();
            var alertEngine = ServiceProvider.GetRequiredService<AlertEngine>();

            foreach (var channel in ServiceProvider.GetServices<IAlertChannel>())
            {
                Result registrationResult = alertEngine.RegisterChannel(channel);
                if (!registrationResult.Success)
                {
                    throw new InvalidOperationException(registrationResult.Message);
                }

                Result initializationResult = channel.InitializeAsync()
                    .GetAwaiter()
                    .GetResult();
                if (!initializationResult.Success)
                {
                    channel.IsEnabled = false;
                    logger.LogError(
                        initializationResult.Exception,
                        "Alert channel {ChannelType} could not be initialized: {Message}",
                        channel.ChannelType,
                        initializationResult.Message);
                }
            }

            try
            {
                using var scope = ServiceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DataContext>();
                database.Database.Migrate();
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, "The AgroEco database could not be migrated.");
                throw;
            }

            _ = jobEngine.Init();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            

        }

        protected override void OnExit(ExitEventArgs e)
        {
            foreach (var channel in ServiceProvider?.GetServices<IAlertChannel>()
                ?? Enumerable.Empty<IAlertChannel>())
            {
                try
                {
                    channel.ShutdownAsync()
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception exception)
                {
                    var logger = ServiceProvider?.GetService<ILogger<App>>();
                    logger?.LogError(
                        exception,
                        "Error shutting down alert channel {ChannelType}",
                        channel.ChannelType);
                }
            }

            (ServiceProvider as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }

}
