using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgroEco.Core.Triggers.Implementations
{
    public class DateTimeTrigger : Trigger
    {
        public DateTimeOffset TargetTime { get; private set; }
        public bool IsActive { get; set; }

        public DateTimeTrigger(string name, DateTimeOffset targetTime) : base(name)
        {
            TargetTime = targetTime;
        }

        public override Result UpdateConfiguration(JsonElement config)
        {
            if (!config.TryGetProperty("targetTime", out JsonElement element)
                || element.ValueKind != JsonValueKind.String
                || !DateTimeOffset.TryParse(
                    element.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out DateTimeOffset targetTime))
            {
                return Result.CreateFailure(
                    "The 'targetTime' configuration value must be a valid date and time.");
            }

            TargetTime = targetTime;
            return Result.CreateSuccess();
        }

        private TimeSpan CalculateAdaptiveInterval(TimeSpan remaining)
        {
            if (remaining.TotalDays > 30) return TimeSpan.FromDays(1);
            if (remaining.TotalDays > 1) return TimeSpan.FromHours(1);
            if (remaining.TotalHours > 1) return TimeSpan.FromMinutes(1);
            if (remaining.TotalMinutes > 1) return TimeSpan.FromSeconds(10);
            return TimeSpan.FromSeconds(1);
        }

        public override async Task<Result> InitTrigger()
        {
           
            if (DateTimeOffset.UtcNow >= TargetTime)
            {
                return Result.CreateFailure("The target time has already passed. Trigger cannot be initialized.");
            }

            try
            {
                while (!ExecutionToken.IsCancellationRequested)
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;

                    if (now >= TargetTime)
                    {
                        if (ExecutionToken.IsCancellationRequested)
                        {
                            return Result.CreateFailure("The operation was canceled.");
                        }

                        List<Result> batchResults = await ExecuteTriggerables();

                        if (batchResults.Any(r => !r.Success))
                        {
                            return Result.CreateFailure("One or more triggers failed during execution.");
                        }

                        return Result.CreateSuccess();
                    }

                    TimeSpan remaining = TargetTime - now;
                    TimeSpan waitInterval = CalculateAdaptiveInterval(remaining);

                    await Task.Delay(waitInterval, ExecutionToken);
                }

                return Result.CreateFailure("The operation was canceled.");
            }
            catch (OperationCanceledException)
            {
                return Result.CreateFailure("The operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result.CreateFailure($"Timer exited unexpectedly: {ex.Message}");
            }
        }
    }
}