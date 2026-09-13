using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using Action = AgroEco.Core.Jobs.Actions.Action;
using AgroEco.Core.Jobs.Triggers;
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
            services.AddScoped<IUnitOfWork, UnitOfWork<DataContext>>();


            return services;
        }
    }
}
