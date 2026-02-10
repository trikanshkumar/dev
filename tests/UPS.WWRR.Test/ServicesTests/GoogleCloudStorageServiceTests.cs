using AutoFixture;
using Google.Apis.Storage.v1.Data;
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

        _service = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, "", 1024 * 1024);
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
        var localFileName = Path.GetTempFileName();

        try
        {
            // Setup GetObjectAsync to return a mock object with size (required by GetFileSizeAsync)
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            await _service.DownloadFile(storageFileName, localFileName);

            _mockStorageClient.Verify(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), null), Times.Once);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
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
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);

        await serviceWithBaseDirectory.GetFileAsString(fileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task DownloadFile_CallsDownloadObjectAsyncWithPrependedDirectory_WhenCalledAndBaseDirectoryIsDefined()
    {
        var baseDirectory = _fixture.Create<string>();
        var fileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);

        try
        {
            // Setup GetObjectAsync to return a mock object with size
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            await serviceWithBaseDirectory.DownloadFile(fileName, localFileName);

            _mockStorageClient.Verify(x => x.GetObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), null), Times.Once);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task GetFileSizeAsync_ReturnsCorrectSize_WhenCalled()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";
        var expectedSize = 12345UL;
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = expectedSize };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.GetFileSizeAsync(fileName);

        Assert.Equal((long)expectedSize, result);
        _mockStorageClient.Verify(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FileExistsAsync_ReturnsTrue_WhenFileExists()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.FileExistsAsync(fileName);

        Assert.True(result);
    }

    [Fact]
    public async Task FileExistsAsync_ReturnsFalse_WhenFileNotFound()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Google.GoogleApiException("Storage", "Not Found") { HttpStatusCode = System.Net.HttpStatusCode.NotFound });

        var result = await _service.FileExistsAsync(fileName);

        Assert.False(result);
    }

}
