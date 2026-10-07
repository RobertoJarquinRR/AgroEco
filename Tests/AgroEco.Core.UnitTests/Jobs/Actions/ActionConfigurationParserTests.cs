using System.Text.Json;
using AgroEco.Core.Alerts;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Configuration;

namespace AgroEco.Core.UnitTests.Jobs.Actions;

public sealed class ActionConfigurationParserTests
{
    private static JsonElement Config(string json)
        => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void Parse_SendAlert_MapsEveryField()
    {
        // Arrange
        JsonElement config = Config("""
        {
            "title": "Riego bajo",
            "message": "El nivel de agua del tanque es crítico",
            "level": "Warning",
            "enableChannels": "database,windows-toast",
            "disableChannels": "windows-toast",
            "options": "{\"actionUrl\": \"/alertas/42\"}"
        }
        """);

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("sendAlert", config);

        // Assert
        SendAlertActionConfiguration alertConfig = Assert.IsType<SendAlertActionConfiguration>(result);
        Assert.Equal("Riego bajo", alertConfig.Title);
        Assert.Equal("El nivel de agua del tanque es crítico", alertConfig.Message);
        Assert.Equal(AlertLevel.Warning, alertConfig.Level);
        Assert.Equal(new[] { "database", "windows-toast" }, alertConfig.EnableChannels);
        Assert.Equal(new[] { "windows-toast" }, alertConfig.DisableChannels);
        Assert.Equal("/alertas/42", alertConfig.Options["actionUrl"]);
    }

    [Fact]
    public void Parse_SendAlert_WithNumericLevel_MapsToLevelEnum()
    {
        // Arrange
        JsonElement config = Config("""{ "title": "t", "message": "m", "level": 3 }""");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("sendAlert", config);

        // Assert
        SendAlertActionConfiguration alertConfig = Assert.IsType<SendAlertActionConfiguration>(result);
        Assert.Equal(AlertLevel.Critical, alertConfig.Level);
    }

    [Fact]
    public void Parse_SendAlert_WithArrayChannels_MapsChannels()
    {
        // Arrange
        JsonElement config = Config("""{ "enableChannels": ["database", "windows-toast"] }""");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("sendAlert", config);

        // Assert
        SendAlertActionConfiguration alertConfig = Assert.IsType<SendAlertActionConfiguration>(result);
        Assert.Equal(new[] { "database", "windows-toast" }, alertConfig.EnableChannels);
    }

    [Fact]
    public void Parse_SendAlert_WithInvalidOptionsJson_ReturnsEmptyOptions()
    {
        // Arrange
        JsonElement config = Config("""{ "options": "{ no es json valido" }""");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("sendAlert", config);

        // Assert
        SendAlertActionConfiguration alertConfig = Assert.IsType<SendAlertActionConfiguration>(result);
        Assert.Empty(alertConfig.Options);
    }

    [Fact]
    public void Parse_SendAlert_WithMissingFields_ReturnsDefaults()
    {
        // Arrange
        JsonElement config = Config("{}");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("sendAlert", config);

        // Assert
        SendAlertActionConfiguration alertConfig = Assert.IsType<SendAlertActionConfiguration>(result);
        Assert.Equal(string.Empty, alertConfig.Title);
        Assert.Equal(string.Empty, alertConfig.Message);
        Assert.Equal(AlertLevel.Info, alertConfig.Level);
        Assert.Empty(alertConfig.EnableChannels);
    }

    [Fact]
    public void Parse_ExecuteTask_MapsNumericFields()
    {
        // Arrange
        JsonElement config = Config("""
        { "insumoId": 7, "cantidadDescontar": 2.5, "costoUnitario": 10, "cultivo": "café" }
        """);

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("executeTask", config);

        // Assert
        ExecuteTaskActionConfiguration execConfig = Assert.IsType<ExecuteTaskActionConfiguration>(result);
        Assert.Equal(7, execConfig.InsumoId);
        Assert.Equal(2.5m, execConfig.CantidadDescontar);
        Assert.Equal(10m, execConfig.CostoUnitario);
        Assert.Equal("café", execConfig.Cultivo);
    }

    [Fact]
    public void Parse_NoOp_ReturnsNoOpConfiguration()
    {
        // Arrange
        JsonElement config = Config("""{ "insumoId": 3 }""");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("noop", config);

        // Assert
        NoOpActionConfiguration noOpConfig = Assert.IsType<NoOpActionConfiguration>(result);
        Assert.Equal(3, noOpConfig.InsumoId);
    }

    [Fact]
    public void Parse_UnknownType_FallsBackToExecuteTaskConfiguration()
    {
        // Arrange
        JsonElement config = Config("""{ "insumoId": 1 }""");

        // Act
        ActionConfiguration result = ActionConfigurationParser.Parse("desconocido", config);

        // Assert
        Assert.IsType<ExecuteTaskActionConfiguration>(result);
    }
}
