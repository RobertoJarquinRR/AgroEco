using System;
using System.Threading;
using System.Threading.Tasks;
using AgroEco.Core.Triggers;
using AgroEco.Core.Triggers.Configuration;
using NCrontab;

namespace AgroEco.Core.Triggers.Implementations;

public class CronTrigger : Trigger
{
    private CronTriggerConfiguration _config = null!;
    private CrontabSchedule? _schedule;

    // EF Core needs a parameterless constructor
    private CronTrigger() : base("") { }

    public CronTrigger(string name, CronTriggerConfiguration config) : base(name)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public CronTriggerConfiguration Config
    {
        get => _config;
        private set
        {
            _config = value;
            _schedule = CrontabSchedule.Parse(value.CronExpression);
        }
    }

    public Result UpdateConfiguration(CronTriggerConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        try
        {
            _schedule = CrontabSchedule.Parse(configuration.CronExpression);
            _config = configuration;
            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            return Result.CreateFailure($"Expresión Cron inválida: {ex.Message}");
        }
    }

    protected override async Task<Result> WaitUntilReadyAsync(
        CancellationToken cancellationToken)
    {
        if (_schedule == null && _config != null)
        {
            _schedule = CrontabSchedule.Parse(_config.CronExpression);
        }
        
        if (_schedule == null)
        {
            return Result.CreateFailure("Schedule not initialized");
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var next = _schedule.GetNextOccurrence(now.DateTime);
            var nextDto = new DateTimeOffset(next, TimeSpan.Zero);

            if (_config.StartDate.HasValue)
            {
                var start = _config.StartDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                if (nextDto < start)
                    nextDto = start;
            }

            if (_config.EndDate.HasValue)
            {
                var end = _config.EndDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
                if (nextDto > end)
                {
                    return Result.CreateFailure("Cron trigger has reached end date");
                }
            }

            if (now >= nextDto)
            {
                return Result.CreateSuccess();
            }

            var remaining = nextDto - now;
            var waitInterval = remaining.TotalSeconds > 60 
                ? TimeSpan.FromSeconds(30) 
                : TimeSpan.FromSeconds(1);

            await Task.Delay(waitInterval, cancellationToken);
        }

        return Result.CreateFailure("The trigger execution was canceled.");
    }
}