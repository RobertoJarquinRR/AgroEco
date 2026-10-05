using AgroEco.Core;
using AgroEco.Core.Interfaces;
using AgroEco.Core.Jobs;
using AgroEco.Core.Jobs.Actions;
using AgroEco.Core.Jobs.Actions.Implementations;
using AgroEco.Core.Jobs.Persistence;
using AgroEco.Core.Triggers.Implementations;
using Moq;

namespace AgroEco.Core.UnitTests.Jobs.Persistence;

public sealed class CreateJobTests
{
    [Fact]
    public async Task HandleAsync_WithUniqueName_AddsJobAndSavesChanges()
    {
        // Arrange
        Mock<IRepository<Job>> repository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        Mock<IRepository<Job>> allRepository = new();
        allRepository
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        GetAllJob getAllJob = new(allRepository.Object);
        CreateJob useCase = new(repository.Object, getAllJob, unitOfWork.Object);
        DateTimeTrigger trigger = new(
            "watering",
            DateTimeOffset.UtcNow.AddHours(1));

        // Act
        Result result = await useCase.HandleAsync(
            "job",
            "description",
            1,
            [new ActionTest("action")],
            trigger);

        // Assert
        Assert.True(result.Success);
        repository.Verify(
            item => item.AddAsync(
                It.Is<Job>(job => job.Name == "job"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        unitOfWork.Verify(
            item => item.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_DoesNotPersistJob()
    {
        // Arrange
        Mock<IRepository<Job>> repository = new();
        Mock<IUnitOfWork> unitOfWork = new();
        Mock<IRepository<Job>> allRepository = new();
        Result<Job> existing = await Job.CreateJob(
            "job",
            null,
            Status.Created,
            null,
            [new ActionTest("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));
        Job existingJob = Assert.IsType<Job>(existing.Value);
        allRepository
            .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingJob]);
        CreateJob useCase = new(
            repository.Object,
            new GetAllJob(allRepository.Object),
            unitOfWork.Object);

        // Act
        Result result = await useCase.HandleAsync(
            "JOB",
            null,
            null,
            [new ActionTest("action")],
            new DateTimeTrigger("watering", DateTimeOffset.UtcNow.AddHours(1)));

        // Assert
        Assert.False(result.Success);
        repository.Verify(
            item => item.AddAsync(
                It.IsAny<Job>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        unitOfWork.Verify(
            item => item.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
