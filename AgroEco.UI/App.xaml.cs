using AgroEco.Data;
using AgroEco.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AgroEco.UI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = default!;
        private IServiceScope? _uiScope;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceProvider = DependencyConfigurator.ConfigureServices();

            using (var scope = ServiceProvider.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<DataContext>();

                database.Database.OpenConnection();
                database.Database.CloseConnection();
                database.Database.Migrate();


            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);


        }
    }

}
