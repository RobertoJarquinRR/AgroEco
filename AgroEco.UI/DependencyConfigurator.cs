using AgroEco.Core;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using AgroEco.UI.Handlers;
using AgroEco.UI.Mensajeros;
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

            services.AddScoped<MainWindow>();
            services.AddScoped<MsgRouter>();

            services.AddScoped<Func<Action<string, object>, TareasHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<TareasHandler>>();
                return new TareasHandler(
                    enviar,
                    sp.GetRequiredService<CreateJob>(),
                    sp.GetRequiredService<GetAllJob>(),
                    sp.GetRequiredService<GetRunningJobs>(),
                    sp.GetRequiredService<GetByIdJob>(),
                    sp.GetRequiredService<UpdateJob>(),
                    sp.GetRequiredService<DeleteJob>(),
                    sp.GetRequiredService<JobEngine>(),
                    logger);
            });

            services.AddScoped<Func<Action<string, object>, PlagasHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<PlagasHandler>>();
                return new PlagasHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, FinanzasHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<FinanzasHandler>>();
                return new FinanzasHandler(enviar, logger);
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

            services.AddScoped<Func<Action<string, object>, InventarioHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<InventarioHandler>>();
                return new InventarioHandler(enviar, logger);
            });

            services.AddScoped<Func<Action<string, object>, EducacionHandler>>(sp => enviar =>
            {
                var logger = sp.GetRequiredService<ILogger<EducacionHandler>>();
                return new EducacionHandler(enviar, logger);
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