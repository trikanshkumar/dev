#nullable enable
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using System.Text;
using UPS.WWRR.Business.Common.Helper;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.Interfaces;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Business.Services;

namespace UPS.WWRR.UnitTests.ServicesTests;

public class CopyBatchDataServiceTest
{
    private readonly Fixture _fixture;
    private readonly Mock<ICsvSplitterService> _csvSplitterMock;
    private readonly Mock<INpgsqlConnectionHelper> _connHelperMock;
    private readonly Mock<ILogger<CopyBatchDataService>> _loggerMock;
    private readonly IConfiguration _config;
    private readonly TableConfigurationRequest _table;
    private readonly string _filePath;
    private readonly string _chunk1;
    private readonly string _chunk2;
    private readonly Mock<ILoadRepository> _loadRepoMock;

    public CopyBatchDataServiceTest()
    {
        // Initialize mocks and fixture first
        _fixture = new Fixture();
        _csvSplitterMock = new Mock<ICsvSplitterService>();
        _connHelperMock = new Mock<INpgsqlConnectionHelper>();
        _loggerMock = new Mock<ILogger<CopyBatchDataService>>();
        _loadRepoMock = new Mock<ILoadRepository>();

        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "2",
            ["BatchLoad:HasHeader"] = "true",
            ["BatchLoad:Delimiter"] = ","
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        _table = new TableConfigurationRequest { TableName = "test_table" };
        _filePath = _fixture.Create<string>();
        _chunk1 = "Id,Name\n1,A\n2,B\n";
        _chunk2 = "Id,Name\n3,C\n4,D\n";

        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { _chunk1, _chunk2 }));

        // Mock OpenConnectionAsync to return a dummy connection (won't be used directly)
        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((NpgsqlConnection)null!);

        // Mock ExecuteNonQueryAsync (TRUNCATE) - just complete successfully
        _connHelperMock.Setup(h => h.ExecuteNonQueryAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Mock BeginTextImportAsync to return a fake TextWriter
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new StringWriter(new StringBuilder()));

        _loadRepoMock.Setup(r => r.AddDetailAsync(It.IsAny<UPS.WWRR.Data.Models.DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Returns<UPS.WWRR.Data.Models.DataLoadDetail, CancellationToken>((d, _) => Task.FromResult(d));

        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<UPS.WWRR.Data.Models.DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private CopyBatchDataService CreateSut() => new(_csvSplitterMock.Object, _loggerMock.Object, _config, _connHelperMock.Object, _loadRepoMock.Object);

    [Fact]
    public async Task CopyAsync_ShouldLoadRows_WhenValidChunks()
    {
        // Arrange done in constructor
        var sut = CreateSut();

        // Act
        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        // Assert
        // 2 chunks × 3 lines each (including header) = 6 attempted (excluding headers = 4, but CountLinesStreaming counts differently)
        // Each chunk: "Id,Name\n1,A\n2,B\n" has 4 lines (3 newlines + 1), minus 1 header = 3 attempted per chunk
        // Actually: CountLinesStreaming("Id,Name\n1,A\n2,B\n") = 4 (1 base + 3 newlines), minus 1 = 3 per chunk, × 2 = 6
        Assert.Equal(6, result.TotalRowsAttempted);
        _csvSplitterMock.Verify(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()), Times.Once);

        // Verify TRUNCATE was called once
        _connHelperMock.Verify(h => h.ExecuteNonQueryAsync(
            It.IsAny<NpgsqlConnection>(),
            It.Is<string>(sql => sql.StartsWith("TRUNCATE TABLE", StringComparison.OrdinalIgnoreCase)),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify BeginTextImportAsync was called for each chunk
        _connHelperMock.Verify(h => h.BeginTextImportAsync(
            It.IsAny<NpgsqlConnection>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify a per-chunk DataLoadDetail was created for each chunk
        _loadRepoMock.Verify(r => r.AddDetailAsync(It.IsAny<UPS.WWRR.Data.Models.DataLoadDetail>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify each per-chunk DataLoadDetail was updated after processing
        _loadRepoMock.Verify(r => r.UpdateDetailAsync(It.IsAny<UPS.WWRR.Data.Models.DataLoadDetail>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CopyAsync_ShouldRecordError_WhenConnectionFails()
    {
        // Override connection to throw
        _connHelperMock.Reset();
        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("OpenConnection failed"));

        var sut = CreateSut();

        // Act
        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.RowsLoaded);
        Assert.Contains(result.Errors, e => e.Contains("OpenConnection failed"));
    }
}
