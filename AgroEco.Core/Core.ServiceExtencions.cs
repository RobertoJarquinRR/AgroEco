using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Creators;
using AgroEco.Core.Jobs.Actions.Persistence;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Jobs.Persistence.Queries;
using AgroEco.Core.Jobs.Runs.Persistence;
using AgroEco.Core.Triggers.Persistence;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Creators;
using AgroEco.Core.Triggers.Implementations;
using AgroEco.Core.Triggers.Events.Persistence;
using AgroEco.Core.Inventario.Persistence;
using AgroEco.Core.Finanzas.Persistence;
using AgroEco.Core.Alertas.Persistence;
using AgroEco.Core.Alertas;
using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
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
            // JobRun persistence
            services.AddScoped<CreateJobRun>();
            services.AddScoped<GetJobRunsByJob>();
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

            // Umbrales de sensores
            services.AddScoped<CreateUmbralSensor>();
            services.AddScoped<GetAllUmbralesSensor>();
            services.AddScoped<GetByIdUmbralSensor>();
            services.AddScoped<UpdateUmbralSensor>();
            services.AddScoped<DeleteUmbralSensor>();

            // Reportes
            services.AddScoped<IExportService, ExportService>();

            // Alerts
            services.AddScoped<CreateAlert>();
            services.AddScoped<GetAlertById>();
            services.AddScoped<GetAllAlerts>();
            services.AddScoped<UpdateAlert>();
            services.AddScoped<DeleteAlert>();

            // TriggerEvent persistence
            services.AddScoped<CreateTriggerEvent>();
            services.AddScoped<GetTriggerEventsByTrigger>();

            services.AddSingleton<ITriggerCreator, DateTimeTriggerCreator>();
            services.AddSingleton<ITriggerCreator, CronTriggerCreator>();
            services.AddSingleton<IActionCreator, NoOpActionCreator>();
            services.AddSingleton<IActionCreator, ExecuteTaskActionCreator>();
            services.AddSingleton<IActionCreator, SendAlertActionCreator>();
            services.AddSingleton<ITriggerFactory, TriggerFactory>();
            services.AddSingleton<IActionFactory, ActionFactory>();

            services.AddSingleton<JobEngine>();
            services.AddSingleton<TriggerEngine>();
            services.AddSingleton<AlertEngine>();

            return services;
        }
    }
}
