using Microsoft.EntityFrameworkCore;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Triggers;
using AgroEco.Core.Jobs.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;

namespace AgroEco.Data
{
    public class DataContext : DbContext
    {
        // DbSet para cada entidad principal de la base de datos
        public DbSet<Job> Jobs { get; set; }
        public DbSet<AgroEco.Core.Jobs.Actions.Action> Actions { get; set; }
        public DbSet<Trigger> Triggers { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo Jobs
            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("Jobs");


                entity.HasKey(j => j.Id);

                entity.Property(j => j.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(j => j.Description)
                      .HasMaxLength(500);

                entity.Property(j => j.Status)
                      .HasConversion<string>();


                entity.HasMany(j => j.Action)
                      .WithOne()
                      .HasForeignKey(j => j.JobId);


                entity.HasOne(j => j.Trigger)
                      .WithMany()
                      .HasForeignKey("TriggerId");


                entity.Ignore(j => j.result);
            });
            //////////////////////////////////////////////////////////////////////////

            // Mapeo de Actions
            modelBuilder.Entity<AgroEco.Core.Jobs.Actions.Action>(entity =>
            {
                entity.ToTable("Actions");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(a => a.Status)
                      .HasConversion<string>();
            });

            modelBuilder.Entity<actionTest>(entity =>
            {
                entity.ToTable("ActionTests");

            });



            ////////////////////////////////////////////////////////////////////////////////

            // Mapeo  Triggers
            modelBuilder.Entity<Trigger>(entity =>
            {
                entity.ToTable("Triggers");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .HasMaxLength(100);

                

            });


            modelBuilder.Entity<DateTimeTrigger>(entity =>{
                entity.ToTable("DateTimeTrigger");
            });
            /////////////////////////////////////////////////////////////////////////////////
        }
    }
}