public sealed class RegistryTests
{
    [Fact]
    public void Register_WhenKeyExists_ReplacesValue()
    {
        // Arrange
        Registry<int, string> registry = new();
        registry.Register(1, "old");

        // Act
        registry.Register(1, "new");

        // Assert
        Assert.Equal("new", registry.Get(1));
        Assert.Single(registry.All());
    }

    [Fact]
    public void Unregister_WhenKeyExists_RemovesValue()
    {
        // Arrange
        Registry<int, string> registry = new();
        registry.Register(1, "value");

        // Act
        bool removed = registry.Unregister(1);

        // Assert
        Assert.True(removed);
        Assert.Null(registry.Get(1));
        Assert.Empty(registry.All());
    }
}
