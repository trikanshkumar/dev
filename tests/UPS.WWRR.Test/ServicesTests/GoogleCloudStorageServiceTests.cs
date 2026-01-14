using AutoFixture;
using Google.Cloud.Storage.V1;
using Moq;
using UPS.WWRR.API.Infrastructure;

namespace UPS.WWRR.UnitTests.ServicesTests;

public class GoogleCloudStorageServiceTests
{
    private readonly Fixture _fixture;

    private readonly Mock<StorageClient> _mockStorageClient;

    private readonly GoogleCloudStorageService _service;

    public GoogleCloudStorageServiceTests()
    {
        _fixture = new Fixture();

        _mockStorageClient = new Mock<StorageClient>();

        _service = new GoogleCloudStorageService(_mockStorageClient.Object);
    }

    [Fact]
    public async Task GetFileAsString_CallsDownloadObjectAsync_WhenCalled()
    {
        var bucketName = _fixture.Create<string>();
        var fileName = $"{_fixture.Create<string>()}.csv";

        await _service.GetFileAsString(bucketName, fileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(bucketName, fileName, It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task DownloadFile_CallsDownloadObjectAsync_WhenCalled()
    {
        var bucketName = _fixture.Create<string>();
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = $"{_fixture.Create<string>()}.csv";

        await _service.DownloadFile(bucketName, storageFileName, localFileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(bucketName, storageFileName, It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task MoveFile_CallsMoveObjectAsync_WhenCalled()
    {
        var bucketName = _fixture.Create<string>();
        var sourceFileName = $"{_fixture.Create<string>()}.csv";
        var destFileName = $"{_fixture.Create<string>()}.csv";

        await _service.MoveFile(bucketName, sourceFileName, destFileName);

        _mockStorageClient.Verify(x => x.MoveObjectAsync(bucketName, sourceFileName, destFileName, null, default), Times.Once);
    }

}
