using AutoFixture;
using Google.Api.Gax;
using Google.Apis.Download;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Moq;
using UPS.WWRR.API.Infrastructure;
using UPS.WWRR.Business.Common.Constants;

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

    #region GetFileAsString

    [Fact]
    public async Task GetFileAsString_CallsDownloadObjectAsync_WhenCalled()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";

        await _service.GetFileAsString(fileName);

        _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, fileName, It.IsAny<Stream>(), null, default, null), Times.Once);
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
    public async Task GetFileAsString_ReturnsFileContent_WhenCalled()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";
        var expectedContent = "col1,col2\nval1,val2";

        _mockStorageClient.Setup(x => x.DownloadObjectAsync(_bucketName, fileName, It.IsAny<Stream>(), null, default, null))
            .Callback<string, string, Stream, DownloadObjectOptions, CancellationToken, IProgress<Google.Apis.Download.IDownloadProgress>>((b, o, stream, opts, ct, p) =>
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(expectedContent);
                stream.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.FromResult<Google.Apis.Storage.v1.Data.Object>(null!));

        var result = await _service.GetFileAsString(fileName);

        Assert.Equal(expectedContent, result);
    }

    #endregion

    #region DownloadFile

    [Fact]
    public async Task DownloadFile_CallsDownloadObjectAsync_WhenCalled()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
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
    public async Task DownloadFile_WillRetryDownloadForMaxRetryCountThenThrowException_WhenDownloadObjectFails()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);
            _mockStorageClient.Setup(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<IDownloadProgress>>()))
                .ThrowsAsync(new Exception());


            await Assert.ThrowsAnyAsync<Exception>(async () => await _service.DownloadFile(storageFileName, localFileName));

            // It should call 6 times. It will fail 5 times, then on the 6th it will throw the exception instead of catching it
            _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), null), Times.Exactly(6));
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task DownloadFile_ThrowsOperationCancelledException_WhenOperationIsCancelled()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);
            _mockStorageClient.Setup(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<IDownloadProgress>>()))
                .ThrowsAsync(new OperationCanceledException());

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await _service.DownloadFile(storageFileName, localFileName, new CancellationToken(true)));
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
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
    public async Task DownloadFile_ThrowsFileNotFoundException_WhenFileNotFoundInBucket()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Google.GoogleApiException("Storage", "Not Found") { HttpStatusCode = System.Net.HttpStatusCode.NotFound });

            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => _service.DownloadFile(storageFileName, localFileName));

            Assert.Contains(storageFileName, ex.Message);
            Assert.Contains(_bucketName, ex.Message);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task DownloadFile_CreatesEmptyFile_WhenFileSizeIsZero()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 0 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            await _service.DownloadFile(storageFileName, localFileName);

            Assert.True(File.Exists(localFileName));
            Assert.Equal(0, new FileInfo(localFileName).Length);
            _mockStorageClient.Verify(x => x.DownloadObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), null), Times.Never);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task DownloadFile_DownloadsMultipleChunks_WhenFileSizeExceedsChunkSize()
    {
        var storageFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();
        var chunkSize = 100;
        var fileSize = 250UL; // 3 chunks: 0-99, 100-199, 200-249
        var smallChunkService = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, "", chunkSize);

        try
        {
            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = fileSize };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, storageFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            await smallChunkService.DownloadFile(storageFileName, localFileName);

            _mockStorageClient.Verify(x => x.DownloadObjectAsync(_bucketName, storageFileName, It.IsAny<Stream>(), It.IsAny<DownloadObjectOptions>(), It.IsAny<CancellationToken>(), null), Times.Exactly(3));
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    #endregion

    #region MoveFile

    [Fact]
    public async Task MoveFile_CallsMoveObjectAsync_WhenCalled()
    {
        var sourceFileName = $"{_fixture.Create<string>()}.csv";
        var destFileName = $"{_fixture.Create<string>()}.csv";

        await _service.MoveFile(sourceFileName, destFileName);

        _mockStorageClient.Verify(x => x.MoveObjectAsync(_bucketName, sourceFileName, destFileName, null, default), Times.Once);
    }

    [Fact]
    public async Task MoveFile_CallsMoveObjectAsyncWithPrependedDirectory_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = _fixture.Create<string>();
        var sourceFileName = $"{_fixture.Create<string>()}.csv";
        var destFileName = $"{_fixture.Create<string>()}.csv";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);

        await serviceWithBaseDirectory.MoveFile(sourceFileName, destFileName);

        _mockStorageClient.Verify(x => x.MoveObjectAsync(_bucketName, $"{baseDirectory}/{sourceFileName}", $"{baseDirectory}/{destFileName}", null, default), Times.Once);
    }

    #endregion

    #region GetFileSizeAsync

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
    public async Task GetFileSizeAsync_ReturnsZero_WhenSizeIsNull()
    {
        var fileName = $"{_fixture.Create<string>()}.csv";
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = null };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.GetFileSizeAsync(fileName);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetFileSizeAsync_UsesBaseDirectory_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = _fixture.Create<string>();
        var fileName = $"{_fixture.Create<string>()}.csv";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 999 };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await serviceWithBaseDirectory.GetFileSizeAsync(fileName);

        Assert.Equal(999, result);
    }

    #endregion

    #region FileExistsAsync

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

    [Fact]
    public async Task FileExistsAsync_UsesBaseDirectory_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = _fixture.Create<string>();
        var fileName = $"{_fixture.Create<string>()}.csv";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await serviceWithBaseDirectory.FileExistsAsync(fileName);

        Assert.True(result);
        _mockStorageClient.Verify(x => x.GetObjectAsync(_bucketName, $"{baseDirectory}/{fileName}", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetFilenameCaseInsensitive

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsNull_WhenNoMatchFound()
    {
        var expectedName = "myfile.csv";
        var objects = new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = "otherfile.csv" },
            new() { Name = "anotherfile.csv" }
        };
        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, null, null))
            .Returns(new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(objects));

        var result = _service.GetFilenameCaseInsensitive(expectedName);

        Assert.Null(result);
    }

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsMatchedName_WhenMatchFoundWithDifferentCasing()
    {
        var expectedName = "MYFILE.CSV";
        var objects = new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = "otherfile.csv" },
            new() { Name = "myfile.csv" }
        };
        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, null, null))
            .Returns(new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(objects));

        var result = _service.GetFilenameCaseInsensitive(expectedName);

        Assert.Equal("myfile.csv", result);
    }

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsMatchedNameWithoutBaseDirectory_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = "uploads";
        var expectedName = "MYFILE.CSV";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);
        var objects = new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = "uploads/myfile.csv" }
        };
        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, null, null))
            .Returns(new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(objects));

        var result = serviceWithBaseDirectory.GetFilenameCaseInsensitive(expectedName);

        Assert.Equal("myfile.csv", result);
    }

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsExactName_WhenExactCasingMatch()
    {
        var expectedName = "myfile.csv";
        var objects = new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = "myfile.csv" }
        };
        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, null, null))
            .Returns(new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(objects));

        var result = _service.GetFilenameCaseInsensitive(expectedName);

        Assert.Equal("myfile.csv", result);
    }

    #endregion

    #region VerifyFileSizeAsync

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsTrue_WhenSizesMatch()
    {
        var remoteFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            var content = "test content data";
            await File.WriteAllTextAsync(localFileName, content);
            var localSize = new FileInfo(localFileName).Length;

            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = (ulong)localSize };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, remoteFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            var result = await _service.VerifyFileSizeAsync(remoteFileName, localFileName);

            Assert.True(result);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsFalse_WhenSizesDiffer()
    {
        var remoteFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(localFileName, "short");

            var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 999999 };
            _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, remoteFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockObject);

            var result = await _service.VerifyFileSizeAsync(remoteFileName, localFileName);

            Assert.False(result);
        }
        finally
        {
            if (File.Exists(localFileName))
                File.Delete(localFileName);
        }
    }

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsFalse_WhenLocalFileDoesNotExist()
    {
        var remoteFileName = $"{_fixture.Create<string>()}.csv";
        var localFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");

        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = 100 };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, remoteFileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.VerifyFileSizeAsync(remoteFileName, localFileName);

        Assert.False(result);
    }

    #endregion

    #region DiscoverReceiptLogFileAsync

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsNull_WhenNoMatchingFiles()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = "unrelated_file.csv" },
            new() { Name = "another_file.txt" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsOldestFile_WhenMultipleMatchingFiles()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_100.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_01_10_200.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_50.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_01_10_200.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_SortsByLoadId_WhenSameDate()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_300.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_100.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_100.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_SkipsFilesWithWrongExtension()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_100.txt" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_SkipsFilesWithTooFewParts()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_SkipsFilesWithNonNumericLoadId()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_ABC.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsNull_WhenAllFilesAreInvalid()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}invalid.txt" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_UsesBaseDirectory_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = "uploads";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"uploads/{ServiceConstants.receiptLogFilePattern}2024_06_15_100.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await serviceWithBaseDirectory.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_100.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsSingleFile_WhenOnlyOneMatch()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_03_20_500.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_03_20_500.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_SkipsFilesWithNonNumericDateParts()
    {
        var asyncEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(new List<Google.Apis.Storage.v1.Data.Object>
        {
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}ABCD_EF_GH_100.csv" },
            new() { Name = $"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv" }
        });
        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, null, null))
            .Returns(asyncEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal($"{ServiceConstants.receiptLogFilePattern}2024_06_15_200.csv", result);
    }

    #endregion

    #region PrependBaseDirectory

    [Fact]
    public void PrependBaseDirectory_ReturnsOriginalName_WhenBaseDirectoryIsEmpty()
    {
        var objectName = "testfile.csv";

        var result = _service.PrependBaseDirectory(objectName);

        Assert.Equal(objectName, result);
    }

    [Fact]
    public void PrependBaseDirectory_ReturnsPrependedName_WhenBaseDirectoryIsDefined()
    {
        var baseDirectory = "mydir";
        var objectName = "testfile.csv";
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, baseDirectory, 1024 * 1024);

        var result = serviceWithBaseDirectory.PrependBaseDirectory(objectName);

        Assert.Equal($"{baseDirectory}/{objectName}", result);
    }

    [Fact]
    public void PrependBaseDirectory_ReturnsOriginalName_WhenBaseDirectoryIsWhitespace()
    {
        var serviceWithWhitespace = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, "   ", 1024 * 1024);
        var objectName = "testfile.csv";

        var result = serviceWithWhitespace.PrependBaseDirectory(objectName);

        Assert.Equal(objectName, result);
    }

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsCaseInsentiveFilename_WhenFound()
    {
        var bucketFilenames = new List<string>
        {
            "ABC.csv",
            "foo.csv",
            "bar.csv"
        };

        var bucketObjects = bucketFilenames.Select(x => new Google.Apis.Storage.v1.Data.Object
        {
            Name = x
        }).ToList();

        var mockEnumerable = new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(bucketObjects);

        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(mockEnumerable);

        var response = _service.GetFilenameCaseInsensitive("abc.csv");
        Assert.NotNull(response);
        Assert.Equal("ABC.csv", response);
    }

    [Fact]
    public void GetFilenameCaseInsensitive_ReturnsNull_WhenNotFound()
    {
        var bucketFilenames = new List<string>
        {
            "ABC.csv",
            "foo.csv",
            "bar.csv"
        };

        var bucketObjects = bucketFilenames.Select(x => new Google.Apis.Storage.v1.Data.Object
        {
            Name = x
        }).ToList();

        var mockEnumerable = new MockEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(bucketObjects);

        _mockStorageClient.Setup(x => x.ListObjects(_bucketName, It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(mockEnumerable);

        var response = _service.GetFilenameCaseInsensitive("go_lions.csv");
        Assert.Null(response);
    }

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsTrue_WhenFileSizeIsCorrect()
    {
        var localFileName = "Resources/SingleLineFile.txt";
        var fileName = $"{_fixture.Create<string>()}.csv";
        var expectedSize = 170UL;
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = expectedSize };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.VerifyFileSizeAsync(fileName, localFileName);

        Assert.True(result);
    }

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsFalse_WhenFileSizeIsIncorrect()
    {
        var localFileName = "Resources/SingleLineFile.txt";
        var fileName = $"{_fixture.Create<string>()}.csv";
        var expectedSize = 1234567UL;
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = expectedSize };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.VerifyFileSizeAsync(fileName, localFileName);

        Assert.False(result);
    }

    [Fact]
    public async Task VerifyFileSizeAsync_ReturnsFalse_WhenLocalFileNotFound()
    {
        var localFileName = "BOGUS_FILE_NAME.zyx";
        var fileName = $"{_fixture.Create<string>()}.csv";
        var expectedSize = 170UL;
        var mockObject = new Google.Apis.Storage.v1.Data.Object { Size = expectedSize };
        _mockStorageClient.Setup(x => x.GetObjectAsync(_bucketName, fileName, It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockObject);

        var result = await _service.VerifyFileSizeAsync(fileName, localFileName);

        Assert.False(result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsReceiptFileName_WhenFound()
    {
        var bucketFilenames = new List<string>
        {
            "WWRR_MOD_RECEIPT_FILES_123456.csv",
            "WWRR_MOD_RECEIPT_FILES_2025_03_19_1",
            "WWRR_MOD_RECEIPT_FILES_2025_03_19_1.csv",
            "foo.csv",
            "bar.csv"
        };

        var bucketObjects = bucketFilenames.Select(x => new Google.Apis.Storage.v1.Data.Object
        {
            Name = x
        }).ToList();

        var mockEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(bucketObjects);


        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(mockEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal("WWRR_MOD_RECEIPT_FILES_2025_03_19_1.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_WillParseLoadIdFromFileNameWithTooManyParts()
    {
        var bucketFilenames = new List<string>
        {
            "WWRR_MOD_RECEIPT_FILES_123456.csv",
            "WWRR_MOD_RECEIPT_FILES_2025_03_19_1",
            "WWRR_MOD_RECEIPT_FILES_2025_03_19_1_12345.csv",
            "foo.csv",
            "bar.csv"
        };

        var bucketObjects = bucketFilenames.Select(x => new Google.Apis.Storage.v1.Data.Object
        {
            Name = x
        }).ToList();

        var mockEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(bucketObjects);


        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(mockEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Equal("WWRR_MOD_RECEIPT_FILES_2025_03_19_1_12345.csv", result);
    }

    [Fact]
    public async Task DiscoverReceiptLogFileAsync_ReturnsNull_WhenNotFound()
    {
        var bucketFilenames = new List<string>
        {
            "WWRR_MOD_RECEIPT_FILES_123456.csv",
            "WWRR_MOD_RECEIPT_FILES_2025_03_19_1",
            "WWRR_MOD_RECEIPT_FILES_2025_03_abcdef.csv",
            "foo.csv",
            "bar.csv"
        };

        var bucketObjects = bucketFilenames.Select(x => new Google.Apis.Storage.v1.Data.Object
        {
            Name = x
        }).ToList();

        var mockEnumerable = new MockAsyncEnumerable<Objects, Google.Apis.Storage.v1.Data.Object>(bucketObjects);


        _mockStorageClient.Setup(x => x.ListObjectsAsync(_bucketName, It.IsAny<string>(), It.IsAny<ListObjectsOptions>()))
            .Returns(mockEnumerable);

        var result = await _service.DiscoverReceiptLogFileAsync();

        Assert.Null(result);
    }

    [Fact]
    public void PrependBaseDirectory_ReturnsStringWithPrependedDirectory_WhenBaseDirectoryIsSet()
    {
        var serviceWithBaseDirectory = new GoogleCloudStorageService(_mockStorageClient.Object, _bucketName, "test", 1024 * 1024);
        var result = serviceWithBaseDirectory.PrependBaseDirectory("example.csv");
        Assert.Equal("test/example.csv", result);
    }

    [Fact]
    public void PrependBaseDirectory_ReturnsTheSameString_WhenBaseDirectoryNotSet()
    {
        var result = _service.PrependBaseDirectory("example.csv");
        Assert.Equal("example.csv", result);
    }

    #endregion
}
