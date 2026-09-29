
using AgroEco.Hardware;
using AgroEco.Core;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Triggers;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Jobs.Triggers.Implementations;
using AgroEco.Core.Jobs;
using AgroEco.Core.Hardware;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyInjection;

using AgroEco.UI;
using AgroEco.Core.Jobs.Triggers.Persistence;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Interfaces;


ServiceProvider Services = DependencyConfigurator.ConfigureServices();

Services = DependencyConfigurator.ConfigureServices();
var job = Services.GetRequiredService<JobEngine>;
var j = job.Invoke();
var o = j.Init();



var createtrigger = Services.GetRequiredService<CreateTrigger>;
var q = createtrigger.Invoke();

var gettrigger = Services.GetRequiredService<CreateJob>;
var get = gettrigger.Invoke();

var save = Services.GetRequiredService<IUnitOfWork>;
var work = save.Invoke();



string isoDateText = "2026-09-19T11:20:00-06:00";
DateTimeOffset parsedTarget = DateTimeOffset.Parse(isoDateText);
DateTimeTrigger timer = new("hora de dormir", parsedTarget);
List<AgroEco.Core.Jobs.Actions.Action> actions = new();

ActionTest n = new("prueba", Status.Created);

actions.Add(n);
var crecion = await get.HandleAsync("prueba 2", "para probar",10, actions,timer);
Console.WriteLine(crecion.Message);

await work.SaveChangesAsync();



var s = await j.RunJob(2);
Console.WriteLine(s.Message);
/*
 
string isoDateText = "2026-09-19T10:38:00-06:00";
DateTimeOffset parsedTarget = DateTimeOffset.Parse(isoDateText);
 DateTimeTrigger timer = new("hora de dormir",parsedTarget);
var  ss=await timer.InitTrigger();

Console.WriteLine(ss.Message); 
*/


Console.ReadLine();