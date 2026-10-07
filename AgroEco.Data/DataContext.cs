using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AgroEco.Core.Jobs;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Alertas;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs.Actions.Configuration;
using System.Text.Json;

namespace AgroEco.Data
{
    public class DataContext : DbContext
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        // DbSet para cada entidad principal de la base de datos
        public DbSet<Job> Jobs { get; set; }
        public DbSet<AgroEco.Core.Jobs.Actions.Action> Actions { get; set; }
        public DbSet<Trigger> Triggers { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<RegistroFinanciero> RegistrosFinancieros { get; set; }
        public DbSet<UmbralSensor> UmbralesSensor { get; set; }
        public DbSet<AlertEntity> Alerts { get; set; }
        public DbSet<AlertDeliveryEntity> AlertDeliveries { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => 
            {
                w.Ignore(RelationalEventId.PendingModelChangesWarning);
                w.Default(WarningBehavior.Log);
            });
        }

        private static SendAlertActionConfiguration DeserializeSendAlertConfig(string json)
        {
            SendAlertActionConfiguration config =
                JsonSerializer.Deserialize<SendAlertActionConfiguration>(json, JsonOptions)
                ?? new SendAlertActionConfiguration();

            if (config.Options is null || config.Options.Count == 0)
            {
                return config;
            }

            var normalized = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, object> pair in config.Options)
            {
                normalized[pair.Key] = NormalizeJsonValue(pair.Value);
            }

            return config with { Options = normalized };
        }

        private static object NormalizeJsonValue(object value)
        {
            if (value is not JsonElement element)
            {
                return value;
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.TryGetInt64(out long integer)
                    ? integer
                    : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => element.GetRawText()
            };
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

                entity.HasDiscriminator<string>("ActionType")
                      .HasValue<NoOpAction>("noop")
                      .HasValue<ExecuteTaskAction>("executeTask")
                      .HasValue<SendAlertAction>("sendAlert");
            });

            modelBuilder.Entity<ExecuteTaskAction>(entity =>
            {
                entity.Property(a => a.Config)
                      .HasColumnName("ExecuteTaskConfig")
                      .HasConversion(
                          value => JsonSerializer.Serialize(value, JsonOptions),
                          json => JsonSerializer.Deserialize<ExecuteTaskActionConfiguration>(json, JsonOptions)
                                  ?? new ExecuteTaskActionConfiguration());
            });

            modelBuilder.Entity<SendAlertAction>(entity =>
            {
                entity.Property(a => a.Config)
                      .HasColumnName("SendAlertConfig")
                      .HasConversion(
                          value => JsonSerializer.Serialize(value, JsonOptions),
                          json => DeserializeSendAlertConfig(json));
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
                
                entity.Property(u => u.AccionTipo)
                      .HasMaxLength(50);
                
                entity.Property(u => u.AccionConfigJson)
                      .HasColumnType("TEXT");
                
                entity.Property(u => u.CooldownMinutos);
                
                entity.Property(u => u.UltimoDisparo);
                
                entity.HasIndex(u => u.SensorTipo);
                entity.HasIndex(u => u.FincaId);
                entity.HasIndex(u => u.Activo);
            });

            modelBuilder.Entity<AlertEntity>(entity =>
            {
                entity.ToTable("Alerts");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(a => a.Message)
                      .HasMaxLength(2000);

                entity.Property(a => a.Level)
                      .IsRequired();

                entity.Property(a => a.Status)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(a => a.ConfigurationJson)
                      .HasColumnType("TEXT");

                entity.Property(a => a.CreatedAt)
                      .IsRequired();

                entity.Property(a => a.DeliveredAt);

                entity.Property(a => a.MetadataJson)
                      .HasColumnType("TEXT");

                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.Level);
                entity.HasIndex(a => a.Status);
            });

            modelBuilder.Entity<AlertDeliveryEntity>(entity =>
            {
                entity.ToTable("AlertDeliveries");
                entity.HasKey(d => d.Id);

                entity.Property(d => d.AlertId)
                      .IsRequired();

                entity.Property(d => d.ChannelType)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.Success)
                      .IsRequired();

                entity.Property(d => d.ErrorMessage)
                      .HasMaxLength(2000);

                entity.Property(d => d.AttemptedAt)
                      .IsRequired();

                entity.Property(d => d.Duration)
                      .IsRequired();

                entity.HasIndex(d => d.AlertId);
                entity.HasIndex(d => d.ChannelType);
            });
            /////////////////////////////////////////////////////////////////////////////////
        }
    }
}
