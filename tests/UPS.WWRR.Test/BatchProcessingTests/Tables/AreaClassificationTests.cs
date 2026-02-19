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
/// Tests for Area Classification tables (TARCLHD and TARCLDT) processing.
/// These tables must be processed together and use a special multi-step merge process.
/// </summary>
public class AreaClassificationTests : BatchProcessorTests
{
    #region BuildLoadsAsync - Paired Table Group Validation Tests

    [Fact]
    public async Task BuildLoadsAsync_BothAreaClassificationTables_ProcessesBoth()
    {
        // Arrange - Both TARCLHD and TARCLDT are present in the receipt
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTARCLHD_2026_02_18_2.csv,SRC\nTARCLDT_2026_02_18_2.csv,SRC";
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
        Assert.Contains(list, l => l.LoadTableName == "TARCLHD");
        Assert.Contains(list, l => l.LoadTableName == "TARCLDT");
        _repo.Verify(r => r.AddLoadsAsync(It.Is<IEnumerable<DataLoad>>(loads => loads.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTARCLHD_SkipsWithWarning()
    {
        // Arrange - Only TARCLHD is present, TARCLDT is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTARCLHD_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TARCLHD should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTARCLDT_SkipsWithWarning()
    {
        // Arrange - Only TARCLDT is present, TARCLHD is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTARCLDT_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TARCLDT should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_MixedTablesWithOnlyTARCLHD_ProcessesOthersSkipsTARCLHD()
    {
        // Arrange - TARCLHD with other tables but no TARCLDT
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2026_02_18_1.csv,SRC\nTARCLHD_2026_02_18_2.csv,SRC\nTDECODE_2026_02_18_3.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TALTCCY and TDECODE should be processed, TARCLHD should be skipped
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == "TALTCCY");
        Assert.Contains(list, l => l.LoadTableName == "TDECODE");
        Assert.DoesNotContain(list, l => l.LoadTableName == "TARCLHD");
    }

    [Fact]
    public async Task BuildLoadsAsync_MixedTablesWithBothAreaClassification_ProcessesAll()
    {
        // Arrange - Both TARCLHD and TARCLDT with other tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2026_02_18_1.csv,SRC\nTARCLHD_2026_02_18_2.csv,SRC\nTARCLDT_2026_02_18_3.csv,SRC\nTDECODE_2026_02_18_4.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - All 4 tables should be processed
        Assert.Equal(4, list.Count);
        Assert.Contains(list, l => l.LoadTableName == "TALTCCY");
        Assert.Contains(list, l => l.LoadTableName == "TARCLHD");
        Assert.Contains(list, l => l.LoadTableName == "TARCLDT");
        Assert.Contains(list, l => l.LoadTableName == "TDECODE");
    }

    [Fact]
    public async Task BuildLoadsAsync_CaseInsensitive_BothAreaClassificationTables()
    {
        // Arrange - Mixed case table names
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\ntarclhd_2026_02_18_2.csv,SRC\nTARCLDT_2026_02_18_2.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - Both should be processed (case insensitive matching)
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task BuildLoadsAsync_NoAreaClassificationTables_ProcessesNormally()
    {
        // Arrange - No Area Classification tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2026_02_18_1.csv,SRC\nTDECODE_2026_02_18_2.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - Both should be processed normally
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == "TALTCCY");
        Assert.Contains(list, l => l.LoadTableName == "TDECODE");
    }

    #endregion

    #region Area Classification Validation Tests

    [Fact]
    public async Task ValidateLoadsAsync_TARCLHD_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 501, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_501.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<AreaClassificationHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TARCLHD_501.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(501, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TARCLDT_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 502, LoadTableName = "TARCLDT", FileLocation = "gs://bucket/TARCLDT_502.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<AreaClassificationDetailDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TARCLDT_502.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(502, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TARCLHD_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 503, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_503.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<AreaClassificationHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid header format" }));
        _storage.Setup(s => s.DownloadFile("TARCLHD_503.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(503, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Area Classification Copy Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TARCLHD_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 511, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_511.csv" };
        _storage.Setup(s => s.DownloadFile("TARCLHD_511.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tarclhd_stg", SourceFile = "temp", RowsLoaded = 100, TotalRowsAttempted = 100, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(511, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TARCLDT_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 512, LoadTableName = "TARCLDT", FileLocation = "gs://bucket/TARCLDT_512.csv" };
        _storage.Setup(s => s.DownloadFile("TARCLDT_512.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tarcldt_stg", SourceFile = "temp", RowsLoaded = 500, TotalRowsAttempted = 500, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(512, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Area Classification Merge Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TARCLHD_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 100, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_100.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(10, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(5, 3, 1, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify staging dataset procedure was called
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(100, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TARCLDT_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 104, LoadTableName = "TARCLDT", FileLocation = "gs://bucket/TARCLDT_104.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(50, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(25, 10, 5, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(104, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_AreaClassification_StagingFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 101, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_101.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_staging", "10", "Staging normalization failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify merge was NOT called and status is Failed
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.UpdateStatusAsync(101, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_AreaClassification_MergeFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 102, LoadTableName = "TARCLDT", FileLocation = "gs://bucket/TARCLDT_102.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(10, 0, 0, null, null, null, null, null)); // Staging succeeds
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_merge", "20", "Merge failed")); // Merge fails
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(102, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_AreaClassification_Success_UpdatesMultipleLoadReferences()
    {
        // Arrange
        var load = new DataLoad { Id = 103, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_103.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(50, 30, 10, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 103, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify UpdateLoadReferenceForMultipleTablesAsync was called with the mapping
        _repo.Verify(r => r.UpdateLoadReferenceForMultipleTablesAsync(
            It.Is<Dictionary<string, string>>(d => d.ContainsKey("tarclhd_stg") && d.ContainsKey("tarcldt_stg")),
            103,
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_RegularTable_DoesNotCallStagingDataset()
    {
        // Arrange - Regular table, not Area Classification
        var load = new DataLoad { Id = 200, LoadTableName = "TALTCCY", FileLocation = "gs://bucket/TALTCCY_200.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(5, 3, 1, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 200, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Staging dataset should NOT be called for regular tables
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 200, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TARCLHD_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 530, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_530.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(10, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", "", "error occurred"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.UpdateStatusAsync(530, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TARCLHD_Success_Processed()
    {
        var load = new DataLoad { Id = 531, LoadTableName = "TARCLHD", FileLocation = "gs://bucket/TARCLHD_531.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(10, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(1, 2, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 531, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.UpdateStatusAsync(531, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ValidateAndFilterPairedTableGroups Direct Tests

    [Fact]
    public void ValidateAndFilterPairedTableGroups_EmptyList_ReturnsEmpty()
    {
        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", new List<DataLoad>());
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_NoPairedTables_ReturnsAll()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TALTCCY" },
            new() { LoadTableName = "TDECODE" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_CompletePair_ReturnsAll()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TARCLHD" },
            new() { LoadTableName = "TARCLDT" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_IncompletePair_RemovesPresent()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TARCLHD" },
            new() { LoadTableName = "TALTCCY" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Single(result);
        Assert.Equal("TALTCCY", result[0].LoadTableName);
    }

    #endregion

    #region Helper Methods

    private static T InvokeSync<T>(object target, string name, params object[] args)
    {
        var mi = target.GetType().GetMethod(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mi == null)
        {
            throw new InvalidOperationException($"Method {name} not found on {target.GetType().Name}.");
        }

        var ret = mi.Invoke(target, args);
        return ret is T value ? value : default!;
    }

    #endregion
}
