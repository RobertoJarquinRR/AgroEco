using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions.Persistence;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Jobs.Triggers.Persistence;
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

            //Trigger persistence
            services.AddScoped<CreateTrigger>();
            services.AddScoped<DeleteTrigger>();
            services.AddScoped<GetAllTrigger>();
            services.AddScoped<GetByIdTrigger>();

            //Action persistence
            services.AddScoped<CreateAction>();
            services.AddScoped<DeleteAction>();
            services.AddScoped<GetAllAction>();
            services.AddScoped<GetByIdAction>();

            

            return services;
        }
    }
}
