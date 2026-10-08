using AgroEco.Core;
using AgroEco.Core.Alerts;
using CommunityToolkit.WinUI.Notifications;
using Windows.UI.Notifications;
using System.Runtime.Versioning;
using System.Threading;

namespace AgroEco.Notifications.Channels;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed class WindowsToastChannel : IAlertChannel
{
    public string ChannelType => "windows-toast";
    public bool IsEnabled { get; set; } = true;
    public int Priority => 10;

    private readonly WindowsToastOptions _options;

    public WindowsToastChannel(WindowsToastOptions? options = null)
    {
        _options = options ?? new WindowsToastOptions();
    }

    public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return Task.FromResult(Result.CreateFailure("Windows Toast notifications require Windows 10 version 1809 or later"));
        }
        return Task.FromResult(Result.CreateSuccess());
    }

    public async Task<AlertDeliveryResult> SendAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var toastContent = BuildToastContent(alert);
            var toast = new ToastNotification(toastContent.GetXml());

            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

            stopwatch.Stop();
            return AlertDeliveryResult.SuccessResult(ChannelType, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var message = string.IsNullOrWhiteSpace(ex.Message) ? ex.ToString() : ex.Message;
            return AlertDeliveryResult.Failure(ChannelType, message, stopwatch.Elapsed);
        }
    }

    private ToastContent BuildToastContent(Alert alert)
    {
        var builder = new ToastContentBuilder()
            .AddText(alert.Title)
            .AddText(alert.Message)
            .SetToastDuration(ToastDuration.Long)
            .AddAudio(new Uri("ms-winsoundevent:Notification.Default"));

        if (_options.ShowAppLogo && TryResolveAppLogo(_options.AppLogoPath, out Uri logo))
        {
            builder.AddAppLogoOverride(logo, ToastGenericAppLogoCrop.Default);
        }

        if (alert.Metadata.TryGetValue("actionUrl", out var actionUrl) && actionUrl is string url)
        {
            builder.AddButton(new ToastButton()
                .SetContent(_options.ActionButtonText)
                .AddArgument("action", "open")
                .AddArgument("url", url));
        }

        return builder.GetToastContent();
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    // ms-appx:/// solo se resuelve en apps empaquetadas; en una app sin empaquetar
    // Windows acepta la notificación pero no pinta el banner si la imagen no existe.
    private static bool TryResolveAppLogo(string path, out Uri logo)
    {
        logo = null!;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (path.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out Uri? absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            logo = absolute;
            return true;
        }

        string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        logo = new Uri(fullPath);
        return true;
    }
}

public sealed class WindowsToastOptions
{
    public string AppId { get; set; } = "AgroEco";
    public string AppLogoPath { get; set; } = "Assets/AppLogo.png";
    public bool ShowAppLogo { get; set; } = true;
    public string ActionButtonText { get; set; } = "Ver detalles";
}