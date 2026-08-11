using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Data;
using AgroEco.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            services.AddScoped<MainWindow>();
            services.AddScoped<JobRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork<DataContext>>();

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
