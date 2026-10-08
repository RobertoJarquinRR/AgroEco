using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using Action = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Triggers;
using AgroEco.Core.Inventario;
using AgroEco.Core.Finanzas;
using AgroEco.Core.Alertas;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs.Runs;
using AgroEco.Core.Triggers.Events;
using AgroEco.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Data
{
    public static class Data
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services){


            services.AddScoped<IRepository<Job>, JobRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IRepository<Trigger>, TriggerRepository>();
            services.AddScoped<IRepository<Action>, ActionRepository>();
            services.AddScoped<IRepository<Insumo>, InsumoRepository>();
            services.AddScoped<IRepository<RegistroFinanciero>, RegistroFinancieroRepository>();
            services.AddScoped<IRepository<UmbralSensor>, UmbralSensorRepository>();
            services.AddScoped<IRepository<AlertEntity>, AlertRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<IRepository<JobRun>, JobRunRepository>();
                        services.AddScoped<IJobRunRepository, JobRunRepository>();
                        services.AddScoped<IRepository<JobRunAction>, JobRunActionRepository>();
                        services.AddScoped<IRepository<TriggerEvent>, TriggerEventRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork<DataContext>>();


            return services;
        }
    }
}
