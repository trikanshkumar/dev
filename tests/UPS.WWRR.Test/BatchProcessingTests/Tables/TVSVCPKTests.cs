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
/// Unit tests for TVSVCPK (Valid Origin Service Package) table batch processing.
/// </summary>
public class TVSVCPKTests : BatchProcessorTests
{
    #region BuildLoadsAsync Tests

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_NoReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_EmptyReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(string.Empty);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_InvalidColumns_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync("Wrong,Header\nval1,val2");
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_ValidSingleLoad_Inserts()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTVSVCPK,TVSVCPK_2026_02_18_1.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVSVCPK", "2026_02_18_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal("TVSVCPK", list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_AlreadyExists_SkipsInsert()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTVSVCPK,TVSVCPK_38002.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVSVCPK", "38002", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVSVCPK_MultipleLoads_InsertsAll()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTVSVCPK,TVSVCPK_2026_02_18_1.csv,SRC\nTVSVCPK,TVSVCPK_2026_02_18_2.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVSVCPK", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal("TVSVCPK", l.LoadTableName));
    }

    #endregion

    #region ValidateLoadsAsync Tests

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 3801, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3801.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3801.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3801, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 3802, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3802.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid export country code" }));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3802.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3802, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_MultipleValidationErrors_RecordsAllErrors()
    {
        var load = new DataLoad { Id = 3803, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3803.csv" };
        var errors = new List<string>
        {
            "Line 2: Invalid GPN_XPT_CNY_CD - exceeds max length",
            "Line 3: Invalid SVC_TYP_CD - exceeds max length",
            "Line 5: Invalid PKG_CHA_TYP_CD - exceeds max length",
            "Line 8: Invalid APV_STS_CD - exceeds max length",
            "Line 10: Invalid REC_EFF_STT_DT - invalid date format"
        };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3803.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3803, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_InvalidExportCountryCode_Fails()
    {
        var load = new DataLoad { Id = 3804, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3804.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_XPT_CNY_CD exceeds maximum length of 4" }));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3804.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3804, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_InvalidServiceTypeCode_Fails()
    {
        var load = new DataLoad { Id = 3805, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3805.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: SVC_TYP_CD exceeds maximum length of 3" }));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3805.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3805, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_InvalidPackageCharacteristicTypeCode_Fails()
    {
        var load = new DataLoad { Id = 3806, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3806.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: PKG_CHA_TYP_CD exceeds maximum length of 3" }));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3806.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3806, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_InvalidApprovalStatusCode_Fails()
    {
        var load = new DataLoad { Id = 3807, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3807.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: APV_STS_CD exceeds maximum length of 2" }));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3807.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3807, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVSVCPK_EmptyFile_SetsReadyToProcess()
    {
        var load = new DataLoad { Id = 3808, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3808.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3808.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3808, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TVSVCPK_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 3811, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3811.csv" };
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3811.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvsvcpk_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3811, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVSVCPK_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 3812, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3812.csv" };
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3812.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = "tvsvcpk_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(3812, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVSVCPK_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 3813, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3813.csv" };
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3813.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvsvcpk_stg",
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
        Assert.Equal("tvsvcpk_stg", capturedConfig!.TableName);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVSVCPK_LargeFile_ProcessesSuccessfully()
    {
        var load = new DataLoad { Id = 3814, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3814.csv" };
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3814.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvsvcpk_stg",
                SourceFile = "temp",
                RowsLoaded = 300000,
                TotalRowsAttempted = 300000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3814, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVSVCPK_PartialFailure_ReportsErrors()
    {
        var load = new DataLoad { Id = 3815, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3815.csv" };
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3815.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var partialResult = new CopyBatchResultDto
        {
            TableName = "tvsvcpk_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(3815, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 3830, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3830.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3830, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_Success_Processed()
    {
        var load = new DataLoad { Id = 3831, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3831.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3831, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 3832, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3832.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 25, null, null, "sp_validoriginservicepackage_merge_proc", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3832, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.MarkStagingCompletedAsync("tvsvcpk_stg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_MergeError_AddsErrorRecord()
    {
        var load = new DataLoad { Id = 3833, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3833.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_validoriginservicepackage_merge_proc", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3833, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_LargeDataSet_Success()
    {
        var load = new DataLoad { Id = 3834, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3834.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(250000, 40000, 10000, null, null, "sp_validoriginservicepackage_merge_proc", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3834, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_OnlyInserts_Success()
    {
        var load = new DataLoad { Id = 3835, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3835.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(500, 0, 0, null, null, "sp_validoriginservicepackage_merge_proc", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3835, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_OnlyUpdates_Success()
    {
        var load = new DataLoad { Id = 3836, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3836.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 200, 0, null, null, "sp_validoriginservicepackage_merge_proc", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3836, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_OnlyDeletes_Success()
    {
        var load = new DataLoad { Id = 3837, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3837.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 150, null, null, "sp_validoriginservicepackage_merge_proc", null, null));
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3837, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVSVCPK_ForeignKeyViolation_Failed()
    {
        var load = new DataLoad { Id = 3838, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3838.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23503", "ERROR", "sp_validoriginservicepackage_merge_proc", "55", "insert or update on table violates foreign key constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3838, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Fact]
    public async Task GetDescriptor_TVSVCPK_ReturnsCorrectDescriptor()
    {
        var load = new DataLoad { Id = 3850, LoadTableName = "TVSVCPK", FileLocation = "gs://bucket/TVSVCPK_3850.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3850.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _validator.Verify(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetDescriptor_TVSVCPK_LowercaseTableName_Works()
    {
        var load = new DataLoad { Id = 3851, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3851.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3851.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3851, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region End-to-End Flow Tests

    [Fact]
    public async Task EndToEnd_TVSVCPK_FullSuccessFlow()
    {
        // Build
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTVSVCPK,TVSVCPK_2026_02_18_1.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVSVCPK", "2026_02_18_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();
        var buildResult = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        Assert.Single(buildResult);
        Assert.Equal("TVSVCPK", buildResult[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TVSVCPK_ValidationThenCopy_Success()
    {
        var load = new DataLoad { Id = 3861, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3861.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3861.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3861, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvsvcpk_stg",
                SourceFile = "temp",
                RowsLoaded = 1000,
                TotalRowsAttempted = 1000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3861, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TVSVCPK_FullPipeline_ValidationToMerge_Success()
    {
        var load = new DataLoad { Id = 3862, LoadTableName = "tvsvcpk", FileLocation = "gs://bucket/TVSVCPK_3862.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginServicePackageDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVSVCPK_3862.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3862, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvsvcpk_stg",
                SourceFile = "temp",
                RowsLoaded = 500,
                TotalRowsAttempted = 500,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3862, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);

        // Merge
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(400, 80, 20, null, null, "sp_validoriginservicepackage_merge_proc", null, null));

        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3862, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
