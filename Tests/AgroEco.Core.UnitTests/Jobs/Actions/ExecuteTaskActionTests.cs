using AgroEco.Core.Finanzas;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Inventario;
using AgroEco.Core.Jobs.Actions.Configuration;
using AgroEco.Core.Jobs.Actions.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AgroEco.Core.UnitTests.Jobs.Actions;

public sealed class ExecuteTaskActionTests
{
    private static Insumo BuildInsumo(
        decimal cantidad = 10m,
        string nombre = "Urea",
        string cultivo = "Maiz",
        string categoria = "Fertilizante",
        string unidad = "kg") => new()
        {
            Id = 1,
            Nombre = nombre,
            Cultivo = cultivo,
            Categoria = categoria,
            Cantidad = cantidad,
            Unidad = unidad
        };

    private static (ServiceProvider Provider,
        Mock<IRepository<Insumo>> InsumoRepository,
        Mock<IRepository<RegistroFinanciero>> RegistroRepository,
        Mock<IUnitOfWork> UnitOfWork) CreateHarness(Insumo? insumo)
    {
        var insumoRepository = new Mock<IRepository<Insumo>>();
        insumoRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insumo);
        insumoRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var registroRepository = new Mock<IRepository<RegistroFinanciero>>();
        registroRepository
            .Setup(r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var services = new ServiceCollection();
        services.AddSingleton(insumoRepository.Object);
        services.AddSingleton(registroRepository.Object);
        services.AddSingleton(unitOfWork.Object);

        return (services.BuildServiceProvider(), insumoRepository, registroRepository, unitOfWork);
    }

    private static ExecuteTaskAction BuildAction(
        ServiceProvider provider,
        ExecuteTaskActionConfiguration config) =>
        new("Accion_Test", provider.GetRequiredService<IServiceScopeFactory>())
        {
            Config = config
        };

    [Fact]
    public async Task Execute_WithEnoughStock_DecrementsInsumoAndRegistersCost()
    {
        // Arrange
        Insumo insumo = BuildInsumo(cantidad: 10m, cultivo: "Maiz", categoria: "Fertilizante");
        (ServiceProvider provider, var insumoRepository, var registroRepository, var unitOfWork) =
            CreateHarness(insumo);
        using (provider)
        {
            RegistroFinanciero? registered = null;
            registroRepository
                .Setup(r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()))
                .Callback<RegistroFinanciero, CancellationToken>((registro, _) => registered = registro)
                .Returns(Task.CompletedTask);

            ExecuteTaskAction action = BuildAction(provider, new ExecuteTaskActionConfiguration
            {
                InsumoId = 1,
                CantidadDescontar = 4m,
                CostoUnitario = 2.5m
            });

            // Act
            Result result = await action.Execute();

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(6m, insumo.Cantidad);
            insumoRepository.Verify(
                r => r.UpdateAsync(insumo, It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.NotNull(registered);
            Assert.Equal("costo", registered!.Tipo);
            Assert.Equal(10m, registered.Monto);
            Assert.Equal("Maiz", registered.Cultivo);
            Assert.Equal("Fertilizante", registered.Categoria);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task Execute_WithConfiguredCultivo_UsesConfiguredValue()
    {
        // Arrange
        Insumo insumo = BuildInsumo(cultivo: "Maiz");
        (ServiceProvider provider, _, var registroRepository, _) = CreateHarness(insumo);
        using (provider)
        {
            RegistroFinanciero? registered = null;
            registroRepository
                .Setup(r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()))
                .Callback<RegistroFinanciero, CancellationToken>((registro, _) => registered = registro)
                .Returns(Task.CompletedTask);

            ExecuteTaskAction action = BuildAction(provider, new ExecuteTaskActionConfiguration
            {
                InsumoId = 1,
                CantidadDescontar = 1m,
                CostoUnitario = 1m,
                Cultivo = "Cacao"
            });

            // Act
            Result result = await action.Execute();

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.NotNull(registered);
            Assert.Equal("Cacao", registered!.Cultivo);
        }
    }

    [Fact]
    public async Task Execute_WithBlankConfiguredCultivo_FallsBackToInsumoCultivo()
    {
        // Arrange
        Insumo insumo = BuildInsumo(cultivo: "Maiz");
        (ServiceProvider provider, _, var registroRepository, _) = CreateHarness(insumo);
        using (provider)
        {
            RegistroFinanciero? registered = null;
            registroRepository
                .Setup(r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()))
                .Callback<RegistroFinanciero, CancellationToken>((registro, _) => registered = registro)
                .Returns(Task.CompletedTask);

            ExecuteTaskAction action = BuildAction(provider, new ExecuteTaskActionConfiguration
            {
                InsumoId = 1,
                CantidadDescontar = 1m,
                CostoUnitario = 1m,
                Cultivo = "  "
            });

            // Act
            Result result = await action.Execute();

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.NotNull(registered);
            Assert.Equal("Maiz", registered!.Cultivo);
        }
    }

    [Fact]
    public async Task Execute_WithInsufficientStock_ReturnsFailureWithoutPersisting()
    {
        // Arrange
        Insumo insumo = BuildInsumo(cantidad: 2m);
        (ServiceProvider provider, var insumoRepository, var registroRepository, _) = CreateHarness(insumo);
        using (provider)
        {
            ExecuteTaskAction action = BuildAction(provider, new ExecuteTaskActionConfiguration
            {
                InsumoId = 1,
                CantidadDescontar = 5m,
                CostoUnitario = 3m
            });

            // Act
            Result result = await action.Execute();

            // Assert
            Assert.False(result.Success);
            Assert.Equal(2m, insumo.Cantidad);
            insumoRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Insumo>(), It.IsAny<CancellationToken>()),
                Times.Never);
            registroRepository.Verify(
                r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }

    [Fact]
    public async Task Execute_WithMissingInsumo_ReturnsFailure()
    {
        // Arrange
        (ServiceProvider provider, _, var registroRepository, _) = CreateHarness(insumo: null);
        using (provider)
        {
            ExecuteTaskAction action = BuildAction(provider, new ExecuteTaskActionConfiguration
            {
                InsumoId = 99,
                CantidadDescontar = 1m,
                CostoUnitario = 1m
            });

            // Act
            Result result = await action.Execute();

            // Assert
            Assert.False(result.Success);
            registroRepository.Verify(
                r => r.AddAsync(It.IsAny<RegistroFinanciero>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
