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

    private readonly string _bucketName;

    public GoogleCloudStorageServiceTests()
    {
        _fixture = new Fixture();

        _mockStorageClient = new Mock<StorageClient>();

        _bucketName = _fixture.Create<string>();

        _service = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, "");
    }

    [Fact]
    public async Task GetFileAsString_CallsDownloadObjectAsync_WhenCalled()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";

        await _service.GetFileAsString(fileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, fileName, It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task DownloadFile_CallsDownloadObjectAsync_WhenCalled()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = $"{_fixture.Create<string>()}.csv";

        await _service.DownloadFile(storageFileName, localFileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task MoveFile_CallsMoveObjectAsync_WhenCalled()
    {
        var sourceFileName = $"{_fixture.Create<string>()}.csv";
        var destFileName = $"{_fixture.Create<string>()}.csv";

        await _service.MoveFile(sourceFileName, destFileName);

        _mockStorageClient.Verify(x => x.MoveObjectAsync(_bucketName, sourceFileName, destFileName, null, default), Times.Once);
    }

    [Fact]
    public async Task GetFileAsString_CallsDownloadObjectAsyncWithPrependedDirectory_WhenCalledAndBaseDirectoryIsDefined()
    {
        var baseDirectory = _fixture.Create<string>();
        var fileName = $"{_fixture.Create<string>()}.csv";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory);

        await serviceWithBaseDirectory.GetFileAsString(fileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<Stream>(), null, default, null), Times.Once);
    }

}
