using AgroEco.Core.Triggers.Configuration;
using System.ComponentModel.DataAnnotations;

namespace AgroEco.Core.Triggers.Configuration;

public sealed record CronTriggerConfiguration : TriggerConfiguration
{
    [Required]
    public string CronExpression { get; init; } = "";
    
    public string? TimeZone { get; init; } = "UTC";
    
    public DateOnly? StartDate { get; init; }
    
    public DateOnly? EndDate { get; init; }
}