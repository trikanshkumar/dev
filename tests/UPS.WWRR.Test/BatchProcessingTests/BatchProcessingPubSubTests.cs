#nullable enable
using Moq;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Services;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing;

public class BatchProcessingPubSubTests : BatchProcessorTests
{
    [Fact]
    public async Task PublishPubSubNotificationAsync_NoProcessedLoads_DoesNotPublish()
    {
        // No loads processed – _processedLoadVersions is empty
        var sut = CreateSut();

        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");

        _pubSub.Verify(
            p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_WithProcessedLoads_PublishesMessage()
    {
        var sut = CreateSut();

        // Simulate processed load versions by adding to the internal list
        var field = sut.GetType().GetField("_processedLoadVersions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var list = (List<string>)field.GetValue(sut)!;
        list.Add("V1");
        list.Add("V2");

        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");

        _pubSub.Verify(
            p => p.PublishMessageAsync(
                It.Is<string>(json => json.Contains("V1") && json.Contains("V2") && json.Contains("test-bucket")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_PubSubServiceNull_DoesNotThrow()
    {
        // Create worker without pub/sub service
        var sut = new BatchProcessorWorker(
            _logger.Object, _storage.Object, _validator.Object,
            _copy.Object, _repo.Object, pubSubService: null);

        var field = sut.GetType().GetField("_processedLoadVersions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var list = (List<string>)field.GetValue(sut)!;
        list.Add("V1");

        // Should not throw
        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_PublishThrows_DoesNotPropagate()
    {
        _pubSub.Setup(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Pub/Sub unavailable"));

        var sut = CreateSut();

        var field = sut.GetType().GetField("_processedLoadVersions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var list = (List<string>)field.GetValue(sut)!;
        list.Add("V1");

        // Should not throw — error is caught and logged
        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");

        _pubSub.Verify(
            p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
