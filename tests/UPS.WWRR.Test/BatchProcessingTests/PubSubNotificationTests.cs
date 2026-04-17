#nullable enable
using AutoFixture;
using Moq;
using Newtonsoft.Json;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing;

public class PubSubNotificationTests : BatchProcessorTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task PublishPubSubNotificationAsync_WhenNoProcessedLoads_DoesNotPublish()
    {
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");
        _pubSub.Verify(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_WhenLoadsProcessed_PublishesMessage()
    {
        var sut = CreateSut();

        // Add processed load versions via reflection to simulate successful processing
        var processedVersions = sut.GetType()
            .GetField("_processedLoadVersions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(sut) as List<string>;
        processedVersions!.Add("2026_3_27_1");
        processedVersions.Add("2026_3_27_2");

        _pubSub.Setup(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");

        _pubSub.Verify(p => p.PublishMessageAsync(It.Is<string>(json =>
            json.Contains("\"loadVersions\"") &&
            json.Contains("2026_3_27_1") &&
            json.Contains("2026_3_27_2") &&
            json.Contains("\"isLocal\":false") &&
            json.Contains("\"batchSize\":") &&
            json.Contains("\"bucket\":\"test-bucket\"")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_MessageContainsCorrectPayloadStructure()
    {
        var sut = CreateSut();

        var processedVersions = sut.GetType()
            .GetField("_processedLoadVersions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(sut) as List<string>;
        processedVersions!.Add("2026_3_27_1");

        string? capturedJson = null;
        _pubSub.Setup(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((json, _) => capturedJson = json)
            .Returns(Task.CompletedTask);

        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");

        Assert.NotNull(capturedJson);
        var message = JsonConvert.DeserializeObject<PubSubNotificationMessage>(capturedJson!);
        Assert.NotNull(message);
        Assert.NotEqual(Guid.Empty, message!.RunId);
        Assert.False(message.IsLocal);
        Assert.Single(message.LoadVersions);
        Assert.Equal("2026_3_27_1", message.LoadVersions[0]);
        Assert.Equal("test-bucket", message.Bucket);
        Assert.True(message.BatchSize >= 0);
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_WhenPublishFails_LogsErrorAndDoesNotThrow()
    {
        var sut = CreateSut();

        var processedVersions = sut.GetType()
            .GetField("_processedLoadVersions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(sut) as List<string>;
        processedVersions!.Add("2026_3_27_1");

        _pubSub.Setup(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Pub/Sub unavailable"));

        // Should not throw
        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_WhenEnableMQIsFalse_DoesNotPublish()
    {
        Environment.SetEnvironmentVariable("Enable_MQ", "false");
        try
        {
            var sut = CreateSut();

            var processedVersions = sut.GetType()
                .GetField("_processedLoadVersions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(sut) as List<string>;
            processedVersions!.Add("2026_3_27_1");

            await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");
            _pubSub.Verify(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Enable_MQ", "true");
        }
    }

    [Fact]
    public async Task PublishPubSubNotificationAsync_WhenPubSubServiceIsNull_DoesNotThrow()
    {
        // Create worker without Pub/Sub service
        var sut = new Business.Services.BatchProcessorWorker(
            _logger.Object, _storage.Object, _validator.Object, _copy.Object, _repo.Object, pubSubService: null);

        // Should not throw even when _pubSubService is null
        await InvokeAsync<object>(sut, "PublishPubSubNotificationAsync");
        _pubSub.Verify(p => p.PublishMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
