using AgroEco.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
