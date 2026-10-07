namespace AgroEco.Core.Alerts;

public sealed record AlertDeliveryResult(
    string ChannelType,
    bool Success,
    string? ErrorMessage,
    DateTime AttemptedAt,
    TimeSpan Duration)
{
    public static AlertDeliveryResult SuccessResult(string channelType, TimeSpan duration)
        => new(channelType, true, null, DateTime.UtcNow, duration);

    public static AlertDeliveryResult Failure(string channelType, string errorMessage, TimeSpan duration)
        => new(channelType, false, errorMessage, DateTime.UtcNow, duration);
}