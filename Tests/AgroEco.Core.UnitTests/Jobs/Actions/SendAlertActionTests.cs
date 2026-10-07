using AgroEco.Core.Alerts;
using AgroEco.Core.Alerts.Persistence;
using AgroEco.Core.Configuration;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Creators;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using CoreAction = AgroEco.Core.Jobs.Actions.Action;

namespace AgroEco.Core.UnitTests.Jobs.Actions;

public sealed class SendAlertActionTests
{
    private static ServiceProvider CreateProvider() => AlertTestProvider.Create();

    [Fact]
    public void Descriptor_HasCorrectTypeIdAndFields()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        SendAlertActionCreator creator = new(engine);

        // Act
        ActionDescriptor descriptor = creator.Descriptor;

        // Assert
        Assert.Equal("sendAlert", descriptor.TypeId);
        Assert.Equal("Enviar alerta", descriptor.DisplayName);
        Assert.Equal(6, descriptor.Fields.Count);
        
        Assert.Contains(descriptor.Fields, f => f.Name == "title");
        Assert.Contains(descriptor.Fields, f => f.Name == "message");
        Assert.Contains(descriptor.Fields, f => f.Name == "level");
        Assert.Contains(descriptor.Fields, f => f.Name == "enableChannels");
        Assert.Contains(descriptor.Fields, f => f.Name == "disableChannels");
        Assert.Contains(descriptor.Fields, f => f.Name == "options");
    }

    [Fact]
    public void Descriptor_ExposesChannelAndLevelChoices()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        SendAlertActionCreator creator = new(engine);

        // Act
        ActionDescriptor descriptor = creator.Descriptor;

        // Assert
        ConfigFieldDescriptor channels = descriptor.Fields.Single(f => f.Name == "enableChannels");
        Assert.True(channels.Multiple);
        Assert.NotNull(channels.Choices);
        Assert.Contains(channels.Choices!, choice => choice.Value == "windows-toast");

        ConfigFieldDescriptor level = descriptor.Fields.Single(f => f.Name == "level");
        Assert.NotNull(level.Choices);
        Assert.Contains(level.Choices!, choice => choice.Value == nameof(AlertLevel.Warning));

        ConfigFieldDescriptor options = descriptor.Fields.Single(f => f.Name == "options");
        Assert.False(options.Required);
    }

    [Fact]
    public void Create_WithValidConfig_ReturnsSendAlertAction()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        SendAlertActionCreator creator = new(engine);
        
        SendAlertActionConfiguration config = new()
        {
            Title = "Test Alert",
            Message = "Test Message",
            Level = AlertLevel.Warning,
            EnableChannels = new List<string> { "database", "windows-toast" },
            Options = new Dictionary<string, object> { ["actionUrl"] = "/test" }
        };

        // Act
        Result<CoreAction> result = creator.Create("Test Action", config);

        // Assert
        Assert.True(result.Success);
        SendAlertAction action = Assert.IsType<SendAlertAction>(result.Value);
        Assert.Equal("Test Action", action.Name);
        Assert.Equal(config.Title, action.Config.Title);
        Assert.Equal(config.Message, action.Config.Message);
        Assert.Equal(config.Level, action.Config.Level);
        Assert.Equal(config.EnableChannels, action.Config.EnableChannels);
        Assert.Equal(config.Options, action.Config.Options);
    }

    [Fact]
    public void Create_WithInvalidConfig_ReturnsFailure()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        SendAlertActionCreator creator = new(engine);
        
        var wrongConfig = new ExecuteTaskActionConfiguration();

        // Act
        Result<CoreAction> result = creator.Create("Test Action", wrongConfig);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid configuration for SendAlertAction", result.Message);
    }
}

public sealed class SendAlertActionExecuteTests
{
    private static ServiceProvider CreateProvider() => AlertTestProvider.Create();

    [Fact]
    public async Task Execute_WithRegisteredChannels_RaisesAlertAndUpdatesStatus()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        
        // Register a test channel that succeeds
        engine.RegisterChannel(new SuccessfulChannel());
        
        SendAlertAction action = new("Test Alert", engine)
        {
            Config = new SendAlertActionConfiguration
            {
                Title = "Test Title",
                Message = "Test Message",
                Level = AlertLevel.Error,
                EnableChannels = new List<string> { "test" }
            }
        };

        // Act
        Result result = await action.Execute();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(AgroEco.Core.Jobs.Status.Succeeded, action.Status);
    }

    [Fact]
    public async Task Execute_WithFailingChannel_ReturnsFailureAndUpdatesStatus()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        
        // Register a test channel that fails
        engine.RegisterChannel(new FailingChannel());
        
        SendAlertAction action = new("Test Alert", engine)
        {
            Config = new SendAlertActionConfiguration
            {
                Title = "Test Title",
                Message = "Test Message",
                Level = AlertLevel.Error,
                EnableChannels = new List<string> { "test" }
            }
        };

        // Act
        Result result = await action.Execute();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(AgroEco.Core.Jobs.Status.Faulted, action.Status);
    }

    [Fact]
    public async Task Execute_WithDisabledChannel_DoesNotDeliver()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        
        // Register channel but don't enable it in config
        engine.RegisterChannel(new SuccessfulChannel());
        
        SendAlertAction action = new("Test Alert", engine)
        {
            Config = new SendAlertActionConfiguration
            {
                Title = "Test Title",
                Message = "Test Message",
                Level = AlertLevel.Error,
                EnableChannels = new List<string> { "database" }, // Not "test"
                DisableChannels = new List<string> { "test" }
            }
        };

        // Act
        Result result = await action.Execute();

        // Assert
        Assert.True(result.Success, result.Message);
        Assert.Contains("no delivery channels", result.Message);
    }

    [Fact]
    public async Task Execute_WithOptions_PassesOptionsToAlert()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        engine.RegisterChannel(new SuccessfulChannel());
        
        SendAlertAction action = new("Test Alert", engine)
        {
            Config = new SendAlertActionConfiguration
            {
                Title = "Test",
                Message = "Test",
                Level = AlertLevel.Info,
                EnableChannels = new List<string> { "test" },
                Options = new Dictionary<string, object> { ["customKey"] = "customValue" }
            }
        };

        // Act
        Result result = await action.Execute();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("customValue", action.Config.Options["customKey"]);
    }

    private sealed class FailingChannel : IAlertChannel
    {
        public string ChannelType => "test";
        public bool IsEnabled { get; set; } = true;
        public int Priority => 1;

        public Task<AlertDeliveryResult> SendAsync(Alert alert, CancellationToken cancellationToken = default)
            => Task.FromResult(AlertDeliveryResult.Failure(ChannelType, "Expected failure", TimeSpan.Zero));

        public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.CreateSuccess());

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class SuccessfulChannel : IAlertChannel
    {
        public string ChannelType => "test";
        public bool IsEnabled { get; set; } = true;
        public int Priority => 1;

        public Task<AlertDeliveryResult> SendAsync(Alert alert, CancellationToken cancellationToken = default)
            => Task.FromResult(AlertDeliveryResult.SuccessResult(ChannelType, TimeSpan.Zero));

        public Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.CreateSuccess());

        public Task ShutdownAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

public sealed class ActionFactoryWithSendAlertTests
{
    private static ServiceProvider CreateProvider() => AlertTestProvider.Create();

    [Fact]
    public void GetAvailable_IncludesSendAlertDescriptor()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        ActionFactory factory = new([
            new NoOpActionCreator(),
            new ExecuteTaskActionCreator(provider.GetRequiredService<IServiceScopeFactory>()),
            new SendAlertActionCreator(engine)
        ]);

        // Act
        IReadOnlyList<ActionDescriptor> descriptors = factory.GetAvailable();

        // Assert
        Assert.Contains(descriptors, d => d.TypeId == "sendAlert");
    }

    [Fact]
    public void Create_SendAlertAction_ReturnsCorrectInstance()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        ActionFactory factory = new([
            new NoOpActionCreator(),
            new ExecuteTaskActionCreator(provider.GetRequiredService<IServiceScopeFactory>()),
            new SendAlertActionCreator(engine)
        ]);
        
        SendAlertActionConfiguration config = new()
        {
            Title = "Test",
            Message = "Test",
            Level = AlertLevel.Info
        };

        // Act
        Result<CoreAction> result = factory.Create("sendAlert", "Test Action", config);

        // Assert
        Assert.True(result.Success);
        Assert.IsType<SendAlertAction>(result.Value);
    }

    [Fact]
    public void Create_WithWrongConfigForSendAlert_ReturnsFailure()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        ActionFactory factory = new([
            new NoOpActionCreator(),
            new ExecuteTaskActionCreator(provider.GetRequiredService<IServiceScopeFactory>()),
            new SendAlertActionCreator(engine)
        ]);

        var wrongConfig = new NoOpActionConfiguration();

        // Act
        Result<CoreAction> result = factory.Create("sendAlert", "Test Action", wrongConfig);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void Create_FromFrontendJsonForSendAlert_Succeeds()
    {
        // Arrange
        using ServiceProvider provider = CreateProvider();
        AlertEngine engine = provider.GetRequiredService<AlertEngine>();
        ActionFactory factory = new([
            new NoOpActionCreator(),
            new ExecuteTaskActionCreator(provider.GetRequiredService<IServiceScopeFactory>()),
            new SendAlertActionCreator(engine)
        ]);

        using JsonDocument document = JsonDocument.Parse("""
        {
            "title": "Riego bajo",
            "message": "Nivel crítico",
            "level": "Warning",
            "enableChannels": "database"
        }
        """);

        // Act
        ActionConfiguration configuration = ActionConfigurationParser.Parse(
            "sendAlert",
            document.RootElement);
        Result<CoreAction> result = factory.Create("sendAlert", "Accion_Riego", configuration);

        // Assert
        Assert.True(result.Success, result.Message);
        Assert.IsType<SendAlertAction>(result.Value);
    }
}

public sealed class SendAlertActionConfigurationTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        SendAlertActionConfiguration config = new();

        // Assert
        Assert.Equal(string.Empty, config.Title);
        Assert.Equal(string.Empty, config.Message);
        Assert.Equal(AlertLevel.Info, config.Level);
        Assert.Empty(config.EnableChannels);
        Assert.Empty(config.DisableChannels);
        Assert.Empty(config.Options);
    }

    [Fact]
    public void WithAllProperties_SetsCorrectly()
    {
        // Act
        SendAlertActionConfiguration config = new()
        {
            Title = "Title",
            Message = "Message",
            Level = AlertLevel.Critical,
            EnableChannels = new List<string> { "database", "windows-toast" },
            DisableChannels = new List<string> { "email" },
            Options = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Assert
        Assert.Equal("Title", config.Title);
        Assert.Equal("Message", config.Message);
        Assert.Equal(AlertLevel.Critical, config.Level);
        Assert.Equal(2, config.EnableChannels.Count);
        Assert.Contains("database", config.EnableChannels);
        Assert.Contains("windows-toast", config.EnableChannels);
        Assert.Contains("email", config.DisableChannels);
        Assert.Equal("value", config.Options["key"]);
    }
}