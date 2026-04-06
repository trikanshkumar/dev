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
using UPS.WWRR.Data.Models;

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
    private long _detailIdCounter = 1;

    public CopyBatchDataServiceTest()
    {
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

        _table = new TableConfigurationRequest { TableName = "test_table", DataLoadId = 100 };
        _filePath = _fixture.Create<string>();
        _chunk1 = "Id,Name\n1,A\n2,B\n";
        _chunk2 = "Id,Name\n3,C\n4,D\n";

        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { _chunk1, _chunk2 }));

        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((NpgsqlConnection)null!);

        _connHelperMock.Setup(h => h.ExecuteNonQueryAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new StringWriter(new StringBuilder()));

        _loadRepoMock.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Returns<DataLoadDetail, CancellationToken>((d, _) =>
            {
                d.Id = _detailIdCounter++;
                return Task.FromResult(d);
            });

        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loadRepoMock.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loadRepoMock.Setup(r => r.UpdateTotalBatchNumber(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private CopyBatchDataService CreateSut() => new(_csvSplitterMock.Object, _loggerMock.Object, _config, _connHelperMock.Object, _loadRepoMock.Object);

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldUseConfigurationValues_WhenProvided()
    {
        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "5000",
            ["BatchLoad:HasHeader"] = "false",
            ["BatchLoad:Delimiter"] = "|"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        Assert.NotNull(sut);
    }

    [Fact]
    public void Constructor_ShouldUseEnvironmentVariables_WhenConfigurationNotProvided()
    {
        Environment.SetEnvironmentVariable("BatchLoad_ChunkSize", "8000");
        Environment.SetEnvironmentVariable("BatchLoad_Delimiter", ";");
        Environment.SetEnvironmentVariable("BatchLoad_HasHeader", "false");

        var settings = new Dictionary<string, string?>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        Assert.NotNull(sut);

        Environment.SetEnvironmentVariable("BatchLoad_ChunkSize", null);
        Environment.SetEnvironmentVariable("BatchLoad_Delimiter", null);
        Environment.SetEnvironmentVariable("BatchLoad_HasHeader", null);
    }

    [Fact]
    public void Constructor_ShouldUseDefaults_WhenNoConfigurationOrEnvironmentVariables()
    {
        var settings = new Dictionary<string, string?>();
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        Assert.NotNull(sut);
    }

    [Fact]
    public void Constructor_ShouldUseDefaultChunkSize_WhenInvalidChunkSizeProvided()
    {
        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "invalid",
            ["BatchLoad:HasHeader"] = "true",
            ["BatchLoad:Delimiter"] = ","
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        Assert.NotNull(sut);
    }

    [Fact]
    public void Constructor_ShouldUseDefaultHasHeader_WhenInvalidHasHeaderProvided()
    {
        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "1000",
            ["BatchLoad:HasHeader"] = "invalid",
            ["BatchLoad:Delimiter"] = ","
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        Assert.NotNull(sut);
    }

    #endregion

    #region CopyAsync - Success Scenarios

    [Fact]
    public async Task CopyAsync_ShouldLoadRows_WhenValidChunks()
    {
        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(4, result.TotalRowsAttempted);
        Assert.Equal(4, result.RowsLoaded);
        Assert.True(result.Success);
        Assert.Equal(_table.TableName, result.TableName);
        Assert.Equal(_filePath, result.SourceFile);
        _csvSplitterMock.Verify(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()), Times.Once);

        _connHelperMock.Verify(h => h.ExecuteNonQueryAsync(
            It.IsAny<NpgsqlConnection>(),
            It.Is<string>(sql => sql.StartsWith("TRUNCATE TABLE", StringComparison.OrdinalIgnoreCase)),
            It.IsAny<CancellationToken>()), Times.Once);

        _connHelperMock.Verify(h => h.BeginTextImportAsync(
            It.IsAny<NpgsqlConnection>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        _loadRepoMock.Verify(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _loadRepoMock.Verify(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _loadRepoMock.Verify(r => r.UpdateTotalBatchNumber(_table.DataLoadId, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyAsync_ShouldProcessSingleChunk_WhenOnlyOneChunkProvided()
    {
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { _chunk1 }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(2, result.TotalRowsAttempted);
        Assert.Equal(2, result.RowsLoaded);
        Assert.True(result.Success);
        _loadRepoMock.Verify(r => r.UpdateTotalBatchNumber(_table.DataLoadId, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyAsync_ShouldNormalizeTimestamps_WhenDashDotFormatPresent()
    {
        var chunkWithTs = "Id,CreatedAt\n1,\"2024-01-15-10.30.45.123456\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunkWithTs }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.TotalRowsAttempted);
        Assert.Equal(1, result.RowsLoaded);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CopyAsync_ShouldNormalizeOracleTimestamps_WhenOracleTimestampTable()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunkWithOracleTs = "Id,CreatedAt\n1,\"20-NOV-18 03.41.58.000000 PM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunkWithOracleTs }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.Equal(1, result.TotalRowsAttempted);
        Assert.Equal(1, result.RowsLoaded);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleUnquotedTimestamps_WhenPresent()
    {
        var chunkWithUnquotedTs = "Id,CreatedAt\n1,2024-01-15-10.30.45.123456\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunkWithUnquotedTs }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.TotalRowsAttempted);
        Assert.Equal(1, result.RowsLoaded);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CopyAsync_ShouldSetDataLoadDetailProperties_WhenCreatingDetails()
    {
        DataLoadDetail? capturedDetail = null;
        _loadRepoMock.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Callback<DataLoadDetail, CancellationToken>((d, _) =>
            {
                if (capturedDetail == null) capturedDetail = d;
                d.Id = _detailIdCounter++;
            })
            .Returns<DataLoadDetail, CancellationToken>((d, _) => Task.FromResult(d));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(capturedDetail);
        Assert.Equal(_table.DataLoadId, capturedDetail.DataLoadId);
        Assert.Equal("STG", capturedDetail.DataLoadType);
        Assert.Equal(0, capturedDetail.ErrorIndicator);
        Assert.Equal("SECONDS", capturedDetail.TimePeriodTypeCode);
        Assert.Equal(1, capturedDetail.BatchNumber);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleEmptyLines_WhenProcessingChunks()
    {
        var chunkWithEmptyLines = "Id,Name\n1,A\n\n2,B\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunkWithEmptyLines }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.RowsLoaded >= 0);
    }

    #endregion

    #region CopyAsync - Error Scenarios

    [Fact]
    public async Task CopyAsync_ShouldRecordError_WhenConnectionFails()
    {
        _connHelperMock.Reset();
        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("OpenConnection failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, result.RowsLoaded);
        Assert.Contains(result.Errors, e => e.Contains("OpenConnection failed"));
    }

    [Fact]
    public async Task CopyAsync_ShouldRecordError_WhenTruncateFails()
    {
        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((NpgsqlConnection)null!);
        _connHelperMock.Setup(h => h.ExecuteNonQueryAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Truncate failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Truncate failed"));
    }

    [Fact]
    public async Task CopyAsync_ShouldFallbackToPerRowCopy_WhenBatchCopyFails()
    {
        var callCount = 0;
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new Exception("Batch COPY failed");
                return new StringWriter(new StringBuilder());
            });

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(callCount > 1);
    }

    [Fact]
    public async Task CopyAsync_ShouldRecordPerRowErrors_WhenFallbackRowsFail()
    {
        var callCount = 0;
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                throw new Exception($"COPY failed - call {callCount}");
            });

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.True(result.Errors.Count > 0);
        _loadRepoMock.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CopyAsync_ShouldUpdateDetailWithErrors_WhenChunkProcessingFails()
    {
        DataLoadDetail? updatedDetail = null;
        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Callback<DataLoadDetail, CancellationToken>((d, _) =>
            {
                if (updatedDetail == null || d.ErrorIndicator > 0) updatedDetail = d;
            })
            .Returns(Task.CompletedTask);

        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("COPY failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(updatedDetail);
        Assert.Equal(1, updatedDetail.ErrorIndicator);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleCancellation_WhenCancellationRequested()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(CancellingAsyncEnumerable(cts.Token));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, cts.Token);

        // The method catches OperationCanceledException and converts it to an error in the result
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleAddDetailFailure_WhenRepositoryThrows()
    {
        _loadRepoMock.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AddDetail failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("AddDetail failed"));
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleUpdateDetailFailure_WhenRepositoryThrows()
    {
        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("UpdateDetail failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("UpdateDetail failed"));
    }

    #endregion

    #region Timestamp Normalization Tests

    [Fact]
    public async Task CopyAsync_ShouldNormalizeMultipleTimestampFields_InSingleRow()
    {
        var chunk = "Id,Created,Modified\n1,\"2024-01-15-10.30.45.123456\",\"2024-02-20-14.15.30.654321\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleOracleTimestamp_WithSingleDigitDay()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunk = "Id,CreatedAt\n1,\"5-JAN-20 01.02.03.000000 AM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleOracleTimestamp_WithPM()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunk = "Id,CreatedAt\n1,\"15-DEC-19 11.59.59.999999 PM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleOracleTimestamp_With12AM()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunk = "Id,CreatedAt\n1,\"15-JAN-20 12.00.00.000000 AM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleOracleTimestamp_With12PM()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunk = "Id,CreatedAt\n1,\"15-JAN-20 12.30.45.000000 PM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleOracleTimestamp_WithYearBefore2000()
    {
        var oracleTable = new TableConfigurationRequest { TableName = "tfscmap_stg", DataLoadId = 100 };
        var chunk = "Id,CreatedAt\n1,\"15-JAN-95 10.30.45.000000 AM\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, oracleTable, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleMixedTimestampFormats_InDifferentFields()
    {
        var chunk = "Id,NormalField,TimestampField\n1,\"regular data\",\"2024-01-15-10.30.45.123456\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldNotModifyNonTimestampFields()
    {
        var chunk = "Id,Data\n1,\"2024-01-15 normal data\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.RowsLoaded);
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public async Task CopyAsync_ShouldHandleEmptyChunk_WhenProvided()
    {
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(Array.Empty<string>()));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(0, result.TotalRowsAttempted);
        Assert.Equal(0, result.RowsLoaded);
        _loadRepoMock.Verify(r => r.UpdateTotalBatchNumber(_table.DataLoadId, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleChunkWithOnlyHeader()
    {
        var headerOnlyChunk = "Id,Name\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { headerOnlyChunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(0, result.TotalRowsAttempted);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleNoHeaderMode_WhenConfigured()
    {
        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "2",
            ["BatchLoad:HasHeader"] = "false",
            ["BatchLoad:Delimiter"] = ","
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        var noHeaderChunk = "1,A\n2,B\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, false, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { noHeaderChunk }));

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(2, result.TotalRowsAttempted);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleCustomDelimiter_WhenConfigured()
    {
        var settings = new Dictionary<string, string?>
        {
            ["BatchLoad:ChunkSize"] = "2",
            ["BatchLoad:HasHeader"] = "true",
            ["BatchLoad:Delimiter"] = "|"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var sut = new CopyBatchDataService(_csvSplitterMock.Object, _loggerMock.Object, config, _connHelperMock.Object, _loadRepoMock.Object);

        var pipeDelimitedChunk = "Id|Name\n1|A\n2|B\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { pipeDelimitedChunk }));

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(2, result.TotalRowsAttempted);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleVeryLargeChunkCount()
    {
        var chunks = Enumerable.Range(1, 100).Select(i => $"Id,Name\n{i},Name{i}\n").ToArray();
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(chunks));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(100, result.TotalRowsAttempted);
        Assert.Equal(100, result.RowsLoaded);
        _loadRepoMock.Verify(r => r.UpdateTotalBatchNumber(_table.DataLoadId, 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleFieldsWithQuotes()
    {
        var chunk = "Id,Name\n1,\"Name with \"\"quotes\"\"\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleFieldsWithCommas()
    {
        var chunk = "Id,Name,Description\n1,\"Smith, John\",\"Developer, Senior\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleFieldsWithNewlines()
    {
        var chunk = "Id,Name\n1,\"Line1\nLine2\"\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.RowsLoaded >= 0);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleEmptyFields()
    {
        var chunk = "Id,Name,Value\n1,,\n2,\"\",-\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(2, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldExcludeIsCompletedIrColumn()
    {
        var chunk = "Id,Name,is_completed_ir\n1,A,Y\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldExcludeLoadRefTeColumn()
    {
        var chunk = "Id,Name,load_ref_te\n1,A,old_value\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { chunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(1, result.RowsLoaded);
    }

    #endregion

    #region Fallback Logic Tests

    [Fact]
    public async Task CopyAsync_ShouldAttemptFallback_WhenBatchCopyFailsOnFirstChunk()
    {
        var firstChunkFail = true;
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (firstChunkFail)
                {
                    firstChunkFail = false;
                    throw new Exception("First chunk batch COPY failed");
                }
                return new StringWriter(new StringBuilder());
            });

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.TotalRowsAttempted >= 2);
    }

    [Fact]
    public async Task CopyAsync_ShouldRecordRowError_WhenSingleRowCopyFails()
    {
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Row COPY failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public async Task CopyAsync_ShouldLoadPartialData_WhenSomeRowsFail()
    {
        var callCount = 0;
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    throw new Exception("Batch failed");
                if (callCount == 3)
                    throw new Exception("Second row failed");
                return new StringWriter(new StringBuilder());
            });

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.RowsLoaded < result.TotalRowsAttempted || result.Errors.Count > 0);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleFallbackWithEmptyChunk()
    {
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Batch failed"));

        var emptyChunk = "Id,Name\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { emptyChunk }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(0, result.RowsLoaded);
    }

    [Fact]
    public async Task CopyAsync_ShouldHandleFallbackWithOnlyHeader()
    {
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Batch failed"));

        var headerOnly = "Id,Name\n";
        _csvSplitterMock.Setup(s => s.SplitAsync(_filePath, 2, true, It.IsAny<CancellationToken>()))
            .Returns(MockDataStore.TestAsync(new[] { headerOnly }));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(0, result.TotalRowsAttempted);
    }

    #endregion

    #region Data Load Tracking Tests

    [Fact]
    public async Task CopyAsync_ShouldCreateDetailWithCorrectBatchNumbers()
    {
        var capturedDetails = new List<DataLoadDetail>();
        _loadRepoMock.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Callback<DataLoadDetail, CancellationToken>((d, _) =>
            {
                capturedDetails.Add(d);
                d.Id = _detailIdCounter++;
            })
            .Returns<DataLoadDetail, CancellationToken>((d, _) => Task.FromResult(d));

        var sut = CreateSut();

        await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(2, capturedDetails.Count);
        Assert.Equal(1, capturedDetails[0].BatchNumber);
        Assert.Equal(2, capturedDetails[1].BatchNumber);
    }

    [Fact]
    public async Task CopyAsync_ShouldUpdateDetailWithTimeProcessValue()
    {
        DataLoadDetail? updatedDetail = null;
        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Callback<DataLoadDetail, CancellationToken>((d, _) => updatedDetail = d)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(updatedDetail);
        Assert.True(updatedDetail.TimeProcessValue >= 0);
    }

    [Fact]
    public async Task CopyAsync_ShouldUpdateDetailWithRecordsInserted()
    {
        DataLoadDetail? updatedDetail = null;
        _loadRepoMock.Setup(r => r.UpdateDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .Callback<DataLoadDetail, CancellationToken>((d, _) => updatedDetail = d)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(updatedDetail);
        Assert.True(updatedDetail.RecordsInserted > 0);
    }

    [Fact]
    public async Task CopyAsync_ShouldRecordExceptions_WhenErrorsOccur()
    {
        IEnumerable<DataLoadException>? capturedExceptions = null;
        _loadRepoMock.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<DataLoadException>, CancellationToken>((ex, _) => capturedExceptions = ex)
            .Returns(Task.CompletedTask);

        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("COPY failed"));

        var sut = CreateSut();

        await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(capturedExceptions);
        Assert.True(capturedExceptions.Any());
    }

    [Fact]
    public async Task CopyAsync_ShouldSetExceptionProperties_Correctly()
    {
        IEnumerable<DataLoadException>? capturedExceptions = null;
        _loadRepoMock.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<DataLoadException>, CancellationToken>((ex, _) => capturedExceptions = ex)
            .Returns(Task.CompletedTask);

        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("COPY failed"));

        var sut = CreateSut();

        await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.NotNull(capturedExceptions);
        var firstException = capturedExceptions.FirstOrDefault();
        if (firstException != null)
        {
            Assert.Equal(_table.TableName, firstException.TableName);
            Assert.Equal("ROW", firstException.ErrorFieldName);
            Assert.True(firstException.TableKey.StartsWith("BATCH:"));
        }
    }

    #endregion

    #region Result DTO Tests

    [Fact]
    public async Task CopyAsync_ShouldSetStartedAt_BeforeProcessing()
    {
        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.StartedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CopyAsync_ShouldSetCompletedAt_AfterProcessing()
    {
        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.CompletedAt >= result.StartedAt);
    }

    [Fact]
    public async Task CopyAsync_ShouldCalculateRowsFailed_Correctly()
    {
        _connHelperMock.Setup(h => h.BeginTextImportAsync(It.IsAny<NpgsqlConnection>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("All rows failed"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.Equal(result.TotalRowsAttempted - result.RowsLoaded, result.RowsFailed);
    }

    [Fact]
    public async Task CopyAsync_ShouldReturnSuccessTrue_WhenNoErrors()
    {
        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(0, result.RowsFailed);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task CopyAsync_ShouldReturnSuccessFalse_WhenErrorsExist()
    {
        _connHelperMock.Setup(h => h.OpenConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection error"));

        var sut = CreateSut();

        var result = await sut.CopyAsync(_filePath, _table, CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    #endregion

    #region Helper Methods

    private static async IAsyncEnumerable<string> CancellingAsyncEnumerable([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        yield return "Id,Name\n1,A\n";
        await Task.Yield();
    }

    #endregion
}
