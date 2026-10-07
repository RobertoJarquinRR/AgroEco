using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AgroEco.Data
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
           .UseSqlite("Data Source= ecoagro.db")
           .ConfigureWarnings(w => 
           {
               w.Ignore(RelationalEventId.PendingModelChangesWarning);
               w.Default(WarningBehavior.Log);
           })
           .Options;

            return new DataContext(options);

        }
    }
}
