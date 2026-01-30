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
/// Unit tests for TVLNSVC (Valid Lane Service) table batch processing.
/// </summary>
public class TVLNSVCTests : BatchProcessorTests
{
    #region BuildLoadsAsync Tests

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_NoReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_EmptyReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(string.Empty);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_InvalidColumns_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync("Wrong,Header\nval1,val2");
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_ValidSingleLoad_Inserts()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVLNSVC_37001.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVLNSVC", 37001, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal("TVLNSVC", list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_AlreadyExists_SkipsInsert()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVLNSVC_37002.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVLNSVC", 37002, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVLNSVC_MultipleLoads_InsertsAll()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVLNSVC_37003.csv,SRC\nTVLNSVC_37004.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVLNSVC", It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal("TVLNSVC", l.LoadTableName));
    }

    #endregion

    #region ValidateLoadsAsync Tests

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 3701, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3701.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3701.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3701, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 3702, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3702.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid export country code" }));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3702.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3702, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_MultipleValidationErrors_RecordsAllErrors()
    {
        var load = new DataLoad { Id = 3703, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3703.csv" };
        var errors = new List<string>
        {
            "Line 2: Invalid GPN_XPT_CNY_CD - exceeds max length",
            "Line 3: Invalid GPN_IPT_CNY_CD - exceeds max length",
            "Line 5: Invalid SVC_TYP_CD - exceeds max length",
            "Line 8: Invalid TBL_ROW_EFF_DT - invalid date format",
            "Line 10: Invalid TBL_ROW_END_DT - invalid date format"
        };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3703.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3703, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_InvalidExportCountryCode_Fails()
    {
        var load = new DataLoad { Id = 3704, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3704.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_XPT_CNY_CD exceeds maximum length of 4" }));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3704.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3704, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_InvalidImportCountryCode_Fails()
    {
        var load = new DataLoad { Id = 3705, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3705.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_IPT_CNY_CD exceeds maximum length of 4" }));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3705.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3705, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_InvalidServiceTypeCode_Fails()
    {
        var load = new DataLoad { Id = 3706, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3706.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: SVC_TYP_CD exceeds maximum length of 3" }));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3706.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3706, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVLNSVC_EmptyFile_SetsReadyToProcess()
    {
        var load = new DataLoad { Id = 3707, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3707.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3707.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3707, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TVLNSVC_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 3711, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3711.csv" };
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3711.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvlnsvc_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3711, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVLNSVC_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 3712, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3712.csv" };
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3712.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = "tvlnsvc_stg",
            SourceFile = "temp",
            RowsLoaded = 0,
            TotalRowsAttempted = 10,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        failedResult.Errors.Add("Copy failed due to data type mismatch");
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3712, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVLNSVC_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 3713, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3713.csv" };
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3713.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvlnsvc_stg",
                SourceFile = "temp",
                RowsLoaded = 5,
                TotalRowsAttempted = 5,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        Assert.NotNull(capturedConfig);
        Assert.Equal("tvlnsvc_stg", capturedConfig!.TableName);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVLNSVC_LargeFile_ProcessesSuccessfully()
    {
        var load = new DataLoad { Id = 3714, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3714.csv" };
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3714.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvlnsvc_stg",
                SourceFile = "temp",
                RowsLoaded = 300000,
                TotalRowsAttempted = 300000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3714, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVLNSVC_PartialFailure_ReportsErrors()
    {
        var load = new DataLoad { Id = 3715, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3715.csv" };
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3715.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var partialResult = new CopyBatchResultDto
        {
            TableName = "tvlnsvc_stg",
            SourceFile = "temp",
            RowsLoaded = 950,
            TotalRowsAttempted = 1000,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        partialResult.Errors.Add("50 rows failed due to constraint violation");
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partialResult);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3715, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 3730, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3730.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3730, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_Success_Processed()
    {
        var load = new DataLoad { Id = 3731, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3731.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 3731, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3731, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 3732, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3732.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 25, null, null, "sp_validlaneservice_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3732, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(175);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3732, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3732, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_MergeError_AddsErrorRecord()
    {
        var load = new DataLoad { Id = 3733, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3733.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_validlaneservice_merge_proc", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3733, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_LargeDataSet_Success()
    {
        var load = new DataLoad { Id = 3734, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3734.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(250000, 40000, 10000, null, null, "sp_validlaneservice_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3734, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(300000);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3734, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_OnlyInserts_Success()
    {
        var load = new DataLoad { Id = 3735, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3735.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(500, 0, 0, null, null, "sp_validlaneservice_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3735, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(500);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3735, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_OnlyUpdates_Success()
    {
        var load = new DataLoad { Id = 3736, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3736.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 200, 0, null, null, "sp_validlaneservice_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3736, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3736, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_OnlyDeletes_Success()
    {
        var load = new DataLoad { Id = 3737, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3737.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 150, null, null, "sp_validlaneservice_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvlnsvc_stg", "tvlnsvc", 3737, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3737, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVLNSVC_ForeignKeyViolation_Failed()
    {
        var load = new DataLoad { Id = 3738, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3738.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23503", "ERROR", "sp_validlaneservice_merge_proc", "55", "insert or update on table violates foreign key constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3738, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Fact]
    public async Task GetDescriptor_TVLNSVC_ReturnsCorrectDescriptor()
    {
        var load = new DataLoad { Id = 3750, LoadTableName = "TVLNSVC", FileLocation = "gs://bucket/TVLNSVC_3750.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3750.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _validator.Verify(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetDescriptor_TVLNSVC_LowercaseTableName_Works()
    {
        var load = new DataLoad { Id = 3751, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3751.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3751.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3751, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region End-to-End Flow Tests

    [Fact]
    public async Task EndToEnd_TVLNSVC_FullSuccessFlow()
    {
        // Build
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVLNSVC_3760.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVLNSVC", 3760, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();
        var buildResult = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        Assert.Single(buildResult);
        Assert.Equal("TVLNSVC", buildResult[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TVLNSVC_ValidationThenCopy_Success()
    {
        var load = new DataLoad { Id = 3761, LoadTableName = "tvlnsvc", FileLocation = "gs://bucket/TVLNSVC_3761.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<ValidLaneServiceDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVLNSVC_3761.csv", It.IsAny<string>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3761, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvlnsvc_stg",
                SourceFile = "temp",
                RowsLoaded = 1000,
                TotalRowsAttempted = 1000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3761, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
