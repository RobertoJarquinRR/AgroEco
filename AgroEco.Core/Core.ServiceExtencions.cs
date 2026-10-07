using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Creators;
using AgroEco.Core.Jobs.Actions.Persistence;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Jobs.Persistence.Queries;
using AgroEco.Core.Triggers.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Creators;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Finanzas.Persistence;
using AgroEco.Core.Alertas.Persistence;
using AgroEco.Core.Alertas;
using AgroEco.Core.Reportes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgroEco.Core
{
    public static class Core
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {

            // Job persistence
            services.AddScoped<CreateJob>();
            services.AddScoped<GetAllJob>();
            services.AddScoped<GetByIdJob>();
            services.AddScoped<GetRunningJobs>();
            services.AddScoped<DeleteJob>();
            services.AddScoped<GetByIdJobWithDetails>();
            services.AddScoped<UpdateJob>();
            services.AddScoped<GetJobsFiltered>();
            services.AddScoped<GetJobExecutionHistory>();
            //Trigger persistence
            services.AddScoped<CreateTrigger>();
            services.AddScoped<DeleteTrigger>();
            services.AddScoped<GetAllTrigger>();
            services.AddScoped<GetByIdTrigger>();
            services.AddScoped<UpdateTrigger>();

            //Action persistence
            services.AddScoped<CreateAction>();
            services.AddScoped<DeleteAction>();
            services.AddScoped<GetAllAction>();
            services.AddScoped<GetByIdAction>();
            services.AddScoped<UpdateAction>();

            // Inventario persistence
            services.AddScoped<CreateInsumo>();
            services.AddScoped<GetAllInsumo>();
            services.AddScoped<GetByIdInsumo>();
            services.AddScoped<UpdateInsumo>();
            services.AddScoped<DeleteInsumo>();

            // Finanzas persistence
            services.AddScoped<CreateRegistroFinanciero>();
            services.AddScoped<GetAllRegistroFinanciero>();
            services.AddScoped<GetByIdRegistroFinanciero>();
            services.AddScoped<UpdateRegistroFinanciero>();
            services.AddScoped<DeleteRegistroFinanciero>();

            // Alertas persistence
            services.AddScoped<CreateAlerta>();
            services.AddScoped<GetAllAlertas>();
            services.AddScoped<GetByIdAlerta>();
            services.AddScoped<UpdateAlerta>();
            services.AddScoped<ResolverAlerta>();
            services.AddScoped<CreateUmbralSensor>();
            services.AddScoped<GetAllUmbralesSensor>();
            services.AddScoped<GetByIdUmbralSensor>();
            services.AddScoped<UpdateUmbralSensor>();
            services.AddScoped<DeleteUmbralSensor>();

            // Reportes
            services.AddScoped<IExportService, ExportService>();

            services.AddSingleton<ITriggerCreator, DateTimeTriggerCreator>();
            services.AddSingleton<ITriggerCreator, CronTriggerCreator>();
            services.AddSingleton<IActionCreator, NoOpActionCreator>();
            services.AddSingleton<IActionCreator, ExecuteTaskActionCreator>();
            services.AddSingleton<ITriggerFactory, TriggerFactory>();
            services.AddSingleton<IActionFactory, ActionFactory>();

            services.AddSingleton<JobEngine>();
            services.AddSingleton<TriggerEngine>();
            services.AddSingleton<AlertEngine>();

            
            return services;
        }
    }
}
