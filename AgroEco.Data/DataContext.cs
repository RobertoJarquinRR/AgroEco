using Microsoft.EntityFrameworkCore;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;

namespace AgroEco.Data
{
    public class DataContext : DbContext
    {
        // DbSet para cada entidad principal de la base de datos
        public DbSet<Job> Jobs { get; set; }
        public DbSet<AgroEco.Core.Jobs.Actions.Action> Actions { get; set; }
        public DbSet<Trigger> Triggers { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<RegistroFinanciero> RegistrosFinancieros { get; set; }

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

                entity.HasIndex(j => j.Status);

                entity.HasMany(j => j.Actions)
                      .WithOne()
                      .HasForeignKey(j => j.JobId);


                entity.HasOne(j => j.Trigger)
                      .WithMany()
                      .HasForeignKey("TriggerId");


                entity.Ignore(j => j.Results);
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

                entity.Ignore(a => a.Configuration);
            });

            modelBuilder.Entity<NoOpAction>(entity =>
            {
                entity.ToTable("ActionTests");

            });



            ////////////////////////////////////////////////////////////////////////////////

            // Mapeo  Triggers
            modelBuilder.Entity<Trigger>(entity =>
            {
                entity.UseTptMappingStrategy();
                entity.ToTable("Triggers");
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                      .HasMaxLength(100);

            });


modelBuilder.Entity<DateTimeTrigger>(entity =>{
                entity.ToTable("DateTimeTrigger");

                entity.Property(t => t.TargetTime);
                
   
            });

            modelBuilder.Entity<CronTrigger>(entity =>{
                entity.ToTable("CronTrigger");

                entity.OwnsOne(t => t.Config, config =>
                {
                    config.Property(c => c.CronExpression)
                          .HasMaxLength(100)
                          .HasColumnName("CronExpression");
                    
                    config.Property(c => c.TimeZone)
                          .HasMaxLength(50)
                          .HasColumnName("TimeZone");
                    
                    config.Property(c => c.StartDate)
                          .HasColumnName("StartDate");
                    
                    config.Property(c => c.EndDate)
                          .HasColumnName("EndDate");
                });
            });
            
            // Mapeo Insumos
            modelBuilder.Entity<Insumo>(entity =>
            {
                entity.ToTable("Insumos");
                entity.HasKey(i => i.Id);
                
                entity.Property(i => i.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(i => i.Categoria)
                      .HasMaxLength(50);
                
                entity.Property(i => i.Cultivo)
                      .HasMaxLength(50);
                
                entity.Property(i => i.Cantidad)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(i => i.Unidad)
                      .HasMaxLength(20);
                
                entity.Property(i => i.StockMin)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(i => i.Finca)
                      .HasMaxLength(50);
                
                entity.Property(i => i.Descripcion)
                      .HasMaxLength(500);
                
                entity.HasIndex(i => i.Nombre);
                entity.HasIndex(i => i.Categoria);
            });
            
            // Mapeo Registros Financieros
            modelBuilder.Entity<RegistroFinanciero>(entity =>
            {
                entity.ToTable("RegistrosFinancieros");
                entity.HasKey(r => r.Id);
                
                entity.Property(r => r.Tipo)
                      .IsRequired()
                      .HasMaxLength(20);
                
                entity.Property(r => r.Cultivo)
                      .HasMaxLength(50);
                
                entity.Property(r => r.Categoria)
                      .HasMaxLength(50);
                
                entity.Property(r => r.Monto)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(r => r.Descripcion)
                      .HasMaxLength(500);
                
                entity.HasIndex(r => r.Fecha);
                entity.HasIndex(r => r.Tipo);
                entity.HasIndex(r => r.TaskId);
            });
            /////////////////////////////////////////////////////////////////////////////////
        }
    }
}