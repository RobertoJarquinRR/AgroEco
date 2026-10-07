using AgroEco.Core;
using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Finanzas.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using AgroEco.Notifications.Channels;
using AgroEco.UI.Events;
using AgroEco.UI.Handlers;
using AgroEco.UI.Alerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroEco.UI
{
    public class DependencyConfigurator
    {
        public static ServiceProvider ConfigureServices()
        {
            ServiceCollection services = new();

            string folder = GetLocalAppFolder();
            System.IO.Directory.CreateDirectory(folder);
            var databasePath = System.IO.Path.Combine(folder, "AgroEco.db");

            services.AddDbContext<DataContext>(db =>
            {
                db.UseSqlite($"Data source={databasePath}");
            });

            services.AddLogging();
            services.AddDataServices();
            services.AddCoreServices();

            services.AddSingleton<IEventBus, InMemoryEventBus>();

            services.AddSingleton<IAlertChannel>(sp =>
            {
                var options = new WindowsToastOptions
                {
                    AppId = "AgroEco",
                    AppLogoPath = "Assets/AppLogo.png"
                };
                return new WindowsToastChannel(options);
            });

            services.AddScoped<MainWindow>();

            services.AddScoped<Func<Action<string, object>, TareasHandler>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TareasHandler>>();
                var createJob = sp.GetRequiredService<CreateJob>();
                var getAllJob = sp.GetRequiredService<GetAllJob>();
                var getRunningJobs = sp.GetRequiredService<GetRunningJobs>();
                var getByIdJob = sp.GetRequiredService<GetByIdJobWithDetails>();
                var updateJob = sp.GetRequiredService<UpdateJob>();
                var deleteJob = sp.GetRequiredService<DeleteJob>();
                var jobEngine = sp.GetRequiredService<JobEngine>();
                var triggerFactory = sp.GetRequiredService<ITriggerFactory>();
                var actionFactory = sp.GetRequiredService<IActionFactory>();
                var getByIdInsumo = sp.GetRequiredService<GetByIdInsumo>();

                return (Action<string, object> enviar) => new TareasHandler(
                    enviar,
                    createJob,
                    getAllJob,
                    getRunningJobs,
                    getByIdJob,
                    updateJob,
                    deleteJob,
                    jobEngine,
                    triggerFactory,
                    actionFactory,
                    getByIdInsumo,
                    logger);
            });

            services.AddScoped<Func<Action<string, object>, PlagasHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<PlagasHandler>>();
                return new PlagasHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, FinanzasHandler>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<FinanzasHandler>>();
                var getAllRegistros = sp.GetRequiredService<GetAllRegistroFinanciero>();
                var createRegistro = sp.GetRequiredService<CreateRegistroFinanciero>();

                return (Action<string, object> enviar) => new FinanzasHandler(
                    enviar,
                    getAllRegistros,
                    createRegistro,
                    logger);
            });

            services.AddScoped<Func<Action<string, object>, DashboardHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<DashboardHandler>>();
                return new DashboardHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, SensoresHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<SensoresHandler>>();
                return new SensoresHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, InventarioHandler>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<InventarioHandler>>();
                var getAllInsumo = sp.GetRequiredService<GetAllInsumo>();
                var createInsumo = sp.GetRequiredService<CreateInsumo>();
                var deleteInsumo = sp.GetRequiredService<DeleteInsumo>();
                var getByIdInsumo = sp.GetRequiredService<GetByIdInsumo>();

                return (Action<string, object> enviar) => new InventarioHandler(
                    enviar,
                    getAllInsumo,
                    createInsumo,
                    deleteInsumo,
                    getByIdInsumo,
                    logger);
            });

            services.AddScoped<Func<Action<string, object>, EducacionHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<EducacionHandler>>();
                return new EducacionHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, AlertHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<AlertHandler>>();
                var alertEngine = sp.GetRequiredService<AlertEngine>();
                var getAllAlerts = sp.GetRequiredService<GetAllAlerts>();
                return new AlertHandler(enviar, logger, alertEngine, getAllAlerts);
            });

            return services.BuildServiceProvider();
        }

        public static string GetLocalAppFolder()
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AgroEco");
            return folder;
        }
    }
}