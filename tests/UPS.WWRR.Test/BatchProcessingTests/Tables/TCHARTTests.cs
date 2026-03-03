#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.LoadTableDto;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

/// <summary>
/// Tests for Rate Chart and Accessorial Rates tables (TCHART and TASYRA) processing.
/// These tables must be processed together and use a special multi-step merge process.
/// </summary>
public class TCHARTTests : BatchProcessorTests
{
    #region BuildLoadsAsync - Paired Table Group Validation Tests

    [Fact]
    public async Task BuildLoadsAsync_BothRateChartTables_ProcessesBoth()
    {
        // Arrange - Both TCHART and TASYRA are present in the receipt
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTCHART,TCHART_2026_02_18_2.csv,SRC\nTASYRA,TASYRA_2026_02_18_2.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == "TCHART");
        Assert.Contains(list, l => l.LoadTableName == "TASYRA");
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTCHART_SkipsWithWarning()
    {
        // Arrange - Only TCHART is present, TASYRA is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTCHART,TCHART_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TCHART should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTASYRA_SkipsWithWarning()
    {
        // Arrange - Only TASYRA is present, TCHART is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTASYRA,TASYRA_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TASYRA should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Rate Chart Copy Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TCHART_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 4311, LoadTableName = "TCHART", FileLocation = "gs://bucket/TCHART_4311.csv" };
        _storage.Setup(s => s.DownloadFile("TCHART_4311.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tchart_stg", SourceFile = "temp", RowsLoaded = 100, TotalRowsAttempted = 100, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(4311, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TASYRA_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 4312, LoadTableName = "TASYRA", FileLocation = "gs://bucket/TASYRA_4312.csv" };
        _storage.Setup(s => s.DownloadFile("TASYRA_4312.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tasyra_stg", SourceFile = "temp", RowsLoaded = 500, TotalRowsAttempted = 500, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(4312, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Rate Chart Merge Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TCHART_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 4320, LoadTableName = "TCHART", FileLocation = "gs://bucket/TCHART_4320.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(20, 10, 5, null, null, null, null, null));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(30);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify staging dataset procedure and merge procedure were both called (2 times total)
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(4320, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_RateChart_StagingFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 4330, LoadTableName = "TCHART", FileLocation = "gs://bucket/TCHART_4330.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_staging", "10", "Staging normalization failed"));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify status is Failed
        _repo.Verify(r => r.UpdateStatusAsync(4330, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_RateChart_MergeFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 4331, LoadTableName = "TASYRA", FileLocation = "gs://bucket/TASYRA_4331.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_merge", "20", "Merge failed"));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.UpdateStatusAsync(4331, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_RateChart_Success_UpdatesMultipleLoadReferences()
    {
        // Arrange
        var load = new DataLoad { Id = 4332, LoadTableName = "TCHART", FileLocation = "gs://bucket/TCHART_4332.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 20, null, null, null, null, null));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify MarkStagingCompletedForMultipleTablesAsync was called
        _repo.Verify(r => r.MarkStagingCompletedForMultipleTablesAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
