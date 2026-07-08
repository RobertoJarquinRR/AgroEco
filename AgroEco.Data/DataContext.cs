using Microsoft.EntityFrameworkCore;
namespace AgroEco.Data
{
    public class DataContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            

        }
    }
}
