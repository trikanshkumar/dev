#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Services;

namespace UPS.WWRR.UnitTests.BatchProcessing;

public class BatchProcessorServiceTests : IDisposable
{
    private readonly string? _previousEnvValue;

    public BatchProcessorServiceTests()
    {
        _previousEnvValue = Environment.GetEnvironmentVariable("LOAD_INTERVAL_MINUTES");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", _previousEnvValue);
    }

    [Fact]
    public void Constructor_ValidInterval_UsesConfiguredValue()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", "7");

        var logger = new Mock<ILogger<BatchProcessor>>();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var sut = new TestableBatchProcessor(logger.Object, scopeFactory.Object);

        var interval = sut.GetIntervalMinutes();

        Assert.Equal(7, interval);
    }

    [Fact]
    public void Constructor_InvalidInterval_UsesDefaultValue()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", "invalid");

        var logger = new Mock<ILogger<BatchProcessor>>();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var sut = new TestableBatchProcessor(logger.Object, scopeFactory.Object);

        var interval = sut.GetIntervalMinutes();

        Assert.Equal(5, interval);
    }

    [Fact]
    public async Task ExecuteAsync_InvokesWorkerProcessAsync()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", "0");

        var logger = new Mock<ILogger<BatchProcessor>>();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();
        var worker = new Mock<IBatchProcessorWorker>();
        using var cts = new CancellationTokenSource();

        scopeFactory.Setup(s => s.CreateScope()).Returns(scope.Object);
        scope.SetupGet(s => s.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IBatchProcessorWorker))).Returns(worker.Object);
        worker.Setup(w => w.ProcessAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(_ => cts.Cancel())
            .Returns(Task.CompletedTask);

        var sut = new TestableBatchProcessor(logger.Object, scopeFactory.Object);

        await sut.RunAsync(cts.Token);

        worker.Verify(w => w.ProcessAsync(It.IsAny<CancellationToken>()), Times.Once);
        scopeFactory.Verify(s => s.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkerThrows_LogsError()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", "0");

        var logger = new Mock<ILogger<BatchProcessor>>();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();
        var worker = new Mock<IBatchProcessorWorker>();
        using var cts = new CancellationTokenSource();
        var ex = new InvalidOperationException("boom");

        scopeFactory.Setup(s => s.CreateScope()).Returns(scope.Object);
        scope.SetupGet(s => s.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IBatchProcessorWorker))).Returns(worker.Object);
        worker.Setup(w => w.ProcessAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(_ => cts.Cancel())
            .ThrowsAsync(ex);

        var sut = new TestableBatchProcessor(logger.Object, scopeFactory.Object);

        await sut.RunAsync(cts.Token);

        logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Unhandled error in BatchProcessor loop.")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkerThrowsOperationCanceled_DoesNotLogError()
    {
        Environment.SetEnvironmentVariable("LOAD_INTERVAL_MINUTES", "0");

        var logger = new Mock<ILogger<BatchProcessor>>();
        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        var serviceProvider = new Mock<IServiceProvider>();
        var worker = new Mock<IBatchProcessorWorker>();
        using var cts = new CancellationTokenSource();

        scopeFactory.Setup(s => s.CreateScope()).Returns(scope.Object);
        scope.SetupGet(s => s.ServiceProvider).Returns(serviceProvider.Object);
        serviceProvider.Setup(s => s.GetService(typeof(IBatchProcessorWorker))).Returns(worker.Object);
        worker.Setup(w => w.ProcessAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(ct =>
            {
                cts.Cancel();
                throw new OperationCanceledException(ct);
            });

        var sut = new TestableBatchProcessor(logger.Object, scopeFactory.Object);

        await sut.RunAsync(cts.Token);

        logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private sealed class TestableBatchProcessor(ILogger<BatchProcessor> logger, IServiceScopeFactory scopeFactory)
        : BatchProcessor(logger, scopeFactory)
    {
        public Task RunAsync(CancellationToken stoppingToken) => ExecuteAsync(stoppingToken);

        public int GetIntervalMinutes()
        {
            var field = typeof(BatchProcessor).GetField("_intervalMinutes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (int)field!.GetValue(this)!;
        }
    }
}
