using Microsoft.EntityFrameworkCore;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Alertas;

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
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<UmbralSensor> UmbralesSensor { get; set; }

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
            
            // Mapeo Alertas
            modelBuilder.Entity<Alerta>(entity =>
            {
                entity.ToTable("Alertas");
                entity.HasKey(a => a.Id);
                
                entity.Property(a => a.Tipo)
                      .IsRequired()
                      .HasMaxLength(50);
                
                entity.Property(a => a.Severidad)
                      .HasMaxLength(20);
                
                entity.Property(a => a.Titulo)
                      .HasMaxLength(100);
                
                entity.Property(a => a.Descripcion)
                      .HasMaxLength(500);
                
                entity.Property(a => a.FincaNombre)
                      .HasMaxLength(50);
                
                entity.Property(a => a.SensorNombre)
                      .HasMaxLength(50);
                
                entity.Property(a => a.SensorTipo)
                      .HasMaxLength(50);
                
                entity.Property(a => a.ValorActual)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(a => a.UmbralConfigurado)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(a => a.AccionSugerida)
                      .HasMaxLength(500);
                
                entity.HasIndex(a => a.EsActiva);
                entity.HasIndex(a => a.FincaId);
                entity.HasIndex(a => a.SensorId);
                entity.HasIndex(a => a.FechaCreacion);
            });
            
            // Mapeo Umbrales Sensor
            modelBuilder.Entity<UmbralSensor>(entity =>
            {
                entity.ToTable("UmbralesSensor");
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.SensorTipo)
                      .IsRequired()
                      .HasMaxLength(50);
                
                entity.Property(u => u.FincaNombre)
                      .HasMaxLength(50);
                
                entity.Property(u => u.Minimo)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(u => u.Maximo)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(u => u.SeveridadMinima)
                      .HasMaxLength(20);
                
                entity.Property(u => u.SeveridadMaxima)
                      .HasMaxLength(20);
                
                entity.Property(u => u.AccionSugerida)
                      .HasMaxLength(500);
                
                entity.Property(u => u.CantidadInsumoSugerida)
                      .HasColumnType("decimal(18,2)");
                
                entity.Property(u => u.CostoUnitarioSugerido)
                      .HasColumnType("decimal(18,2)");
                
                entity.HasIndex(u => u.SensorTipo);
                entity.HasIndex(u => u.FincaId);
                entity.HasIndex(u => u.Activo);
            });
            /////////////////////////////////////////////////////////////////////////////////
        }
    }
}