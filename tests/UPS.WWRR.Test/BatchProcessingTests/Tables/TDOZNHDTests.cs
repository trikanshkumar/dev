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
/// Tests for Domestic Zone tables (TDOZNHD and TDOZNDT) processing.
/// These tables must be processed together and use a special multi-step merge process.
/// </summary>
public class TDOZNHDTests : BatchProcessorTests
{
    #region BuildLoadsAsync - Paired Table Group Validation Tests

    [Fact]
    public async Task BuildLoadsAsync_BothDomesticZoneTables_ProcessesBoth()
    {
        // Arrange - Both TDOZNHD and TDOZNDT are present in the receipt
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTDOZNHD_2026_02_18_2.csv,SRC\nTDOZNDT_2026_02_18_2.csv,SRC";
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
        Assert.Contains(list, l => l.LoadTableName == "TDOZNHD");
        Assert.Contains(list, l => l.LoadTableName == "TDOZNDT");
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTDOZNHD_SkipsWithWarning()
    {
        // Arrange - Only TDOZNHD is present, TDOZNDT is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTDOZNHD_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TDOZNHD should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTDOZNDT_SkipsWithWarning()
    {
        // Arrange - Only TDOZNDT is present, TDOZNHD is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTDOZNDT_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TDOZNDT should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Domestic Zone Copy Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TDOZNHD_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 4211, LoadTableName = "TDOZNHD", FileLocation = "gs://bucket/TDOZNHD_4211.csv" };
        _storage.Setup(s => s.DownloadFile("TDOZNHD_4211.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tdoznhd_stg", SourceFile = "temp", RowsLoaded = 50, TotalRowsAttempted = 50, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(4211, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TDOZNDT_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 4212, LoadTableName = "TDOZNDT", FileLocation = "gs://bucket/TDOZNDT_4212.csv" };
        _storage.Setup(s => s.DownloadFile("TDOZNDT_4212.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tdozndt_stg", SourceFile = "temp", RowsLoaded = 200, TotalRowsAttempted = 200, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(4212, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Domestic Zone Merge Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TDOZNHD_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 4220, LoadTableName = "TDOZNHD", FileLocation = "gs://bucket/TDOZNHD_4220.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(10, 5, 2, null, null, null, null, null));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify staging dataset procedure and merge procedure were both called (2 times total)
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(4220, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_DomesticZone_StagingFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 4230, LoadTableName = "TDOZNHD", FileLocation = "gs://bucket/TDOZNHD_4230.csv" };
        
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
        _repo.Verify(r => r.UpdateStatusAsync(4230, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_DomesticZone_MergeFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 4231, LoadTableName = "TDOZNDT", FileLocation = "gs://bucket/TDOZNDT_4231.csv" };
        
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
        _repo.Verify(r => r.UpdateStatusAsync(4231, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_DomesticZone_Success_UpdatesMultipleLoadReferences()
    {
        // Arrange
        var load = new DataLoad { Id = 4232, LoadTableName = "TDOZNHD", FileLocation = "gs://bucket/TDOZNHD_4232.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(50, 30, 10, null, null, null, null, null));
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.MarkStagingCompletedForMultipleTablesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(80);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify MarkStagingCompletedForMultipleTablesAsync was called with the mapping keys
        _repo.Verify(r => r.MarkStagingCompletedForMultipleTablesAsync(
            It.Is<IEnumerable<string>>(keys => keys.Any(k => k.Contains("tdoznhd_stg")) && keys.Any(k => k.Contains("tdozndt_stg"))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
