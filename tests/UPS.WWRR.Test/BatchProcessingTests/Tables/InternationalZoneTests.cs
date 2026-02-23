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
/// Tests for International Zone tables (TINZNHD and TINZNDT) processing.
/// These tables must be processed together and use a special multi-step merge process
/// with staging dataset normalization.
/// </summary>
public class InternationalZoneTests : BatchProcessorTests
{
    #region BuildLoadsAsync - Paired Table Group Validation Tests

    [Fact]
    public async Task BuildLoadsAsync_BothInternationalZoneTables_ProcessesBoth()
    {
        // Arrange - Both TINZNHD and TINZNDT are present in the receipt
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTINZNHD_2025_01_15_12345.csv,SRC\nTINZNDT_2025_01_15_12345.csv,SRC";
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
        Assert.Contains(list, l => l.LoadTableName == "TINZNHD");
        Assert.Contains(list, l => l.LoadTableName == "TINZNDT");
        _repo.Verify(r => r.AddLoadsAsync(It.Is<IEnumerable<DataLoad>>(loads => loads.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTINZNHD_SkipsWithWarning()
    {
        // Arrange - Only TINZNHD is present, TINZNDT is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTINZNHD_2025_01_15_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TINZNHD should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_OnlyTINZNDT_SkipsWithWarning()
    {
        // Arrange - Only TINZNDT is present, TINZNHD is missing
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTINZNDT_2025_01_15_12345.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TINZNDT should be skipped
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_MixedTablesWithOnlyTINZNHD_ProcessesOthersSkipsTINZNHD()
    {
        // Arrange - TINZNHD with other tables but no TINZNDT
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2025_01_15_11111.csv,SRC\nTINZNHD_2025_01_15_12345.csv,SRC\nTDECODE_2025_01_15_22222.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _storage.Setup(s => s.PrependBaseDirectory(It.IsAny<string>())).Returns<string>(s => s);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert - TALTCCY and TDECODE should be processed, TINZNHD should be skipped
        Assert.Equal(2, list.Count);
        Assert.Contains(list, l => l.LoadTableName == "TALTCCY");
        Assert.Contains(list, l => l.LoadTableName == "TDECODE");
        Assert.DoesNotContain(list, l => l.LoadTableName == "TINZNHD");
    }

    [Fact]
    public async Task BuildLoadsAsync_MixedTablesWithBothInternationalZone_ProcessesAll()
    {
        // Arrange - Both TINZNHD and TINZNDT with other tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2025_01_15_11111.csv,SRC\nTINZNHD_2025_01_15_12345.csv,SRC\nTINZNDT_2025_01_15_12345.csv,SRC\nTDECODE_2025_01_15_22222.csv,SRC";
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
        Assert.Contains(list, l => l.LoadTableName == "TINZNHD");
        Assert.Contains(list, l => l.LoadTableName == "TINZNDT");
        Assert.Contains(list, l => l.LoadTableName == "TDECODE");
    }

    [Fact]
    public async Task BuildLoadsAsync_CaseInsensitive_BothInternationalZoneTables()
    {
        // Arrange - Mixed case table names
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\ntinznhd_2025_01_15_12345.csv,SRC\nTINZNDT_2025_01_15_12345.csv,SRC";
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
    public async Task BuildLoadsAsync_NoInternationalZoneTables_ProcessesNormally()
    {
        // Arrange - No International Zone tables
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTALTCCY_2025_01_15_11111.csv,SRC\nTDECODE_2025_01_15_22222.csv,SRC";
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

    #region International Zone Validation Tests

    [Fact]
    public async Task ValidateLoadsAsync_TINZNHD_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 601, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_601.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINZNHD_601.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(601, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINZNDT_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 602, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_602.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneDetailDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINZNDT_602.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(602, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINZNHD_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 603, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_603.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid ORG_CNY_CD format" }));
        _storage.Setup(s => s.DownloadFile("TINZNHD_603.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(603, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINZNDT_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 604, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_604.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneDetailDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid DEL_ZN_NR format" }));
        _storage.Setup(s => s.DownloadFile("TINZNDT_604.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(604, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINZNHD_MultipleValidationErrors_RecordsAll()
    {
        var load = new DataLoad { Id = 605, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_605.csv" };
        var errors = new List<string> 
        { 
            "Invalid ORG_CNY_CD: must be 2 characters",
            "Invalid SVC_TYP_CD: must be 3 characters",
            "Invalid ZCH_NR: must be numeric"
        };
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile("TINZNHD_605.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(e => e.Count() == 3), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region International Zone Copy Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNHD_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 611, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_611.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNHD_611.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinznhd_stg", SourceFile = "temp", RowsLoaded = 1000, TotalRowsAttempted = 1000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(611, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNDT_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 612, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_612.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNDT_612.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinzndt_stg", SourceFile = "temp", RowsLoaded = 5000, TotalRowsAttempted = 5000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(612, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNHD_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 613, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_613.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNHD_613.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == "tinznhd_stg"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinznhd_stg", SourceFile = "temp", RowsLoaded = 100, TotalRowsAttempted = 100, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _copy.Verify(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == "tinznhd_stg"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNDT_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 614, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_614.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNDT_614.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == "tinzndt_stg"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinzndt_stg", SourceFile = "temp", RowsLoaded = 500, TotalRowsAttempted = 500, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _copy.Verify(c => c.CopyAsync(It.IsAny<string>(), It.Is<TableConfigurationRequest>(r => r.TableName == "tinzndt_stg"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNHD_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 615, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_615.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNHD_615.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto 
        { 
            TableName = "tinznhd_stg", 
            SourceFile = "temp", 
            RowsLoaded = 50, 
            TotalRowsAttempted = 100, 
            StartedAt = DateTimeOffset.UtcNow, 
            CompletedAt = DateTimeOffset.UtcNow
        };
        failedResult.Errors.Add("Copy operation failed");
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        _repo.Verify(r => r.UpdateStatusAsync(615, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINZNHD_LargeFile_ProcessesSuccessfully()
    {
        var load = new DataLoad { Id = 616, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_616.csv" };
        _storage.Setup(s => s.DownloadFile("TINZNHD_616.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto 
            { 
                TableName = "tinznhd_stg", 
                SourceFile = "temp", 
                RowsLoaded = 6800000, 
                TotalRowsAttempted = 6800000, 
                StartedAt = DateTimeOffset.UtcNow, 
                CompletedAt = DateTimeOffset.UtcNow
            });
        
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        // Should NOT set Failed status when copy succeeds
        _repo.Verify(r => r.UpdateStatusAsync(616, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region International Zone Merge Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TINZNHD_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 700, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_700.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify stored procedures were called (staging normalization + merge)
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(700, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINZNDT_CallsStagingDatasetProcedure()
    {
        // Arrange
        var load = new DataLoad { Id = 701, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_701.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(701, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_InternationalZone_StagingNormalizationFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 702, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_702.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_internationalzone_stagingdataset_proc", "10", "Staging normalization failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify merge was NOT called (staging failed) and status is Failed
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(702, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_InternationalZone_MergeFails_SetsFailedStatus()
    {
        // Arrange
        var load = new DataLoad { Id = 703, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_703.csv" };
        
        // First call (staging) succeeds, second call (merge) fails
        _repo.SetupSequence(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null)) // Staging succeeds
            .ReturnsAsync(new MergeResult(0, 0, 0, "42000", "ERROR", "sp_internationalzone_merge_proc", "20", "Merge failed")); // Merge fails
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _repo.Verify(r => r.UpdateStatusAsync(703, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_InternationalZone_Success_UpdatesMultipleLoadReferences()
    {
        // Arrange
        var load = new DataLoad { Id = 704, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_704.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 100, 50, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 704, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);

        var sut = CreateSut();

        // Act
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);

        // Assert - Verify UpdateLoadReferenceForMultipleTablesAsync was called with the international zone mapping
        _repo.Verify(r => r.UpdateLoadReferenceForMultipleTablesAsync(
            It.Is<Dictionary<string, string>>(d => d.ContainsKey("tinznhd_stg") && d.ContainsKey("tinzndt_stg") && d.ContainsKey("izchartsts_stg")),
            704,
            It.IsAny<long>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINZNHD_FailedMerge_RecordsError()
    {
        var load = new DataLoad { Id = 730, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_730.csv" };
        
        _repo.SetupSequence(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6800000, 0, 0, null, null, null, null, null))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_internationalzone_merge_proc", "", "Duplicate key violation"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(730, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINZNHD_Success_MarksProcessed()
    {
        var load = new DataLoad { Id = 731, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_731.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 731, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.UpdateStatusAsync(731, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINZNDT_Success_MarksProcessed()
    {
        var load = new DataLoad { Id = 732, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_732.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 732, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.UpdateStatusAsync(732, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_InternationalZone_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 733, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_733.csv" };
        
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6000000, 500000, 100000, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 733, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        
        _repo.Verify(r => r.UpdateStatusAsync(733, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Fact]
    public void GetDescriptor_TINZNHD_ReturnsCorrectDescriptor()
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", "TINZNHD");
        
        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        var stagingTableName = descriptor.GetType().GetProperty("StagingTableName")?.GetValue(descriptor)?.ToString();
        var requiresNormalization = (bool)(descriptor.GetType().GetProperty("RequiresStagingNormalization")?.GetValue(descriptor) ?? false);
        
        Assert.Equal("tinznhd", tableName);
        Assert.Equal("tinznhd_stg", stagingTableName);
        Assert.True(requiresNormalization);
    }

    [Fact]
    public void GetDescriptor_TINZNDT_ReturnsCorrectDescriptor()
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", "TINZNDT");
        
        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        var stagingTableName = descriptor.GetType().GetProperty("StagingTableName")?.GetValue(descriptor)?.ToString();
        var requiresNormalization = (bool)(descriptor.GetType().GetProperty("RequiresStagingNormalization")?.GetValue(descriptor) ?? false);
        
        Assert.Equal("tinzndt", tableName);
        Assert.Equal("tinzndt_stg", stagingTableName);
        Assert.True(requiresNormalization);
    }

    [Fact]
    public void GetDescriptor_TINZNHD_LowercaseTableName_Works()
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", "tinznhd");
        
        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        Assert.Equal("tinznhd", tableName);
    }

    [Fact]
    public void GetDescriptor_TINZNDT_LowercaseTableName_Works()
    {
        var sut = CreateSut();
        var descriptor = InvokeSync<object>(sut, "GetDescriptor", "tinzndt");
        
        var tableName = descriptor.GetType().GetProperty("TableName")?.GetValue(descriptor)?.ToString();
        Assert.Equal("tinzndt", tableName);
    }

    #endregion

    #region ValidateAndFilterPairedTableGroups Tests for International Zone

    [Fact]
    public void ValidateAndFilterPairedTableGroups_CompletePair_TINZNHD_TINZNDT_ReturnsAll()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TINZNHD" },
            new() { LoadTableName = "TINZNDT" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.LoadTableName == "TINZNHD");
        Assert.Contains(result, l => l.LoadTableName == "TINZNDT");
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_IncompletePair_TINZNHD_Only_RemovesIt()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TINZNHD" },
            new() { LoadTableName = "TALTCCY" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Single(result);
        Assert.Equal("TALTCCY", result[0].LoadTableName);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_IncompletePair_TINZNDT_Only_RemovesIt()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TINZNDT" },
            new() { LoadTableName = "TDECODE" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Single(result);
        Assert.Equal("TDECODE", result[0].LoadTableName);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_MultiplePairedGroups_AllComplete_ReturnsAll()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TINZNHD" },
            new() { LoadTableName = "TINZNDT" },
            new() { LoadTableName = "TARCLHD" },
            new() { LoadTableName = "TARCLDT" },
            new() { LoadTableName = "TALTCCY" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void ValidateAndFilterPairedTableGroups_MultiplePairedGroups_OneIncomplete_RemovesIncomplete()
    {
        var loads = new List<DataLoad>
        {
            new() { LoadTableName = "TINZNHD" },
            new() { LoadTableName = "TINZNDT" },
            new() { LoadTableName = "TARCLHD" }, // TARCLDT is missing
            new() { LoadTableName = "TALTCCY" }
        };

        var sut = CreateSut();
        var result = InvokeSync<List<DataLoad>>(sut, "ValidateAndFilterPairedTableGroups", loads);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, l => l.LoadTableName == "TINZNHD");
        Assert.Contains(result, l => l.LoadTableName == "TINZNDT");
        Assert.Contains(result, l => l.LoadTableName == "TALTCCY");
        Assert.DoesNotContain(result, l => l.LoadTableName == "TARCLHD");
    }

    #endregion

    #region End-to-End Flow Tests

    [Fact]
    public async Task EndToEnd_TINZNHD_FullSuccessFlow()
    {
        // Arrange
        var load = new DataLoad { Id = 800, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_800.csv" };
        
        // Validation succeeds
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINZNHD_800.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        // Copy succeeds
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinznhd_stg", SourceFile = "temp", RowsLoaded = 6800000, TotalRowsAttempted = 6800000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        // Merge succeeds
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 800, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        
        var sut = CreateSut();
        
        // Act - Validate
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(800, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
        
        // Act - Copy
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(800, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
        
        // Act - Merge
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(800, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TINZNDT_FullSuccessFlow()
    {
        // Arrange
        var load = new DataLoad { Id = 801, LoadTableName = "TINZNDT", FileLocation = "gs://bucket/TINZNDT_801.csv" };
        
        // Validation succeeds
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneDetailDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINZNDT_801.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        // Copy succeeds
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinzndt_stg", SourceFile = "temp", RowsLoaded = 6800000, TotalRowsAttempted = 6800000, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        
        // Merge succeeds
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(6822838, 0, 0, null, null, null, null, null));
        _repo.Setup(r => r.UpdateLoadReferenceForMultipleTablesAsync(It.IsAny<Dictionary<string, string>>(), 801, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        
        var sut = CreateSut();
        
        // Act - Validate
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(801, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
        
        // Act - Copy
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(801, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
        
        // Act - Merge
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(801, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_InternationalZone_ValidationFails_DoesNotProceed()
    {
        // Arrange
        var load = new DataLoad { Id = 802, LoadTableName = "TINZNHD", FileLocation = "gs://bucket/TINZNHD_802.csv" };
        
        // Validation fails
        _validator.Setup(v => v.ValidateCsvAsync<InternationalZoneHeaderDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid data format" }));
        _storage.Setup(s => s.DownloadFile("TINZNHD_802.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        var sut = CreateSut();
        
        // Act - Validate
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        
        // Assert - Validation failed, no copy or merge should happen
        _repo.Verify(r => r.UpdateStatusAsync(802, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
        _copy.Verify(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
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
