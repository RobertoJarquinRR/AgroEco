using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroEco.Core.Triggers.Configuration;

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

        public DateTimeTrigger(
            string name,
            DateTimeTriggerConfiguration configuration)
            : this(name, configuration.TargetTime)
        {
        }

        public Result UpdateConfiguration(DateTimeTriggerConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (configuration.TargetTime <= DateTimeOffset.UtcNow)
            {
                return Result.CreateFailure(
                    "The target time must be in the future.");
            }

            TargetTime = configuration.TargetTime;
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

                        Result[] failedResults = batchResults
                            .Where(result => !result.Success)
                            .ToArray();
                        if (failedResults.Length > 0)
                        {
                            string details = string.Join(
                                "; ",
                                failedResults
                                    .Select(result => result.Message)
                                    .Where(message => !string.IsNullOrWhiteSpace(message)));
                            return Result.CreateFailure(
                                string.IsNullOrWhiteSpace(details)
                                    ? "One or more triggers failed during execution."
                                    : $"One or more triggers failed during execution: {details}");
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