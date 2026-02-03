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
/// Unit tests for TVORGBT (Valid Origin Bill Term) table batch processing.
/// </summary>
public class TVORGBTTests : BatchProcessorTests
{
    #region BuildLoadsAsync Tests

    [Fact]
    public async Task BuildLoadsAsync_TVORGBT_NoReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVORGBT_EmptyReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(string.Empty);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVORGBT_InvalidColumns_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync("Wrong,Header\nval1,val2");
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVORGBT_ValidSingleLoad_Inserts()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVORGBT_36001.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVORGBT", 36001, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal("TVORGBT", list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVORGBT_AlreadyExists_SkipsInsert()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVORGBT_36002.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVORGBT", 36002, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ValidateLoadsAsync Tests

    [Fact]
    public async Task ValidateLoadsAsync_TVORGBT_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 3601, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3601.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginBillTermDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TVORGBT_3601.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3601, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVORGBT_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 3602, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3602.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginBillTermDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid export country code" }));
        _storage.Setup(s => s.DownloadFile("TVORGBT_3602.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3602, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVORGBT_MultipleValidationErrors_RecordsAllErrors()
    {
        var load = new DataLoad { Id = 3603, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3603.csv" };
        var errors = new List<string>
        {
            "Line 2: Invalid GPN_XPT_CNY_CD",
            "Line 3: Invalid BIL_TER_TYP_CD",
            "Line 5: Invalid APV_STS_CD"
        };
        _validator.Setup(v => v.ValidateCsvAsync<ValidOriginBillTermDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile("TVORGBT_3603.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3603, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TVORGBT_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 3611, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3611.csv" };
        _storage.Setup(s => s.DownloadFile("TVORGBT_3611.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvorgbt_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3611, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVORGBT_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 3612, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3612.csv" };
        _storage.Setup(s => s.DownloadFile("TVORGBT_3612.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = "tvorgbt_stg",
            SourceFile = "temp",
            RowsLoaded = 0,
            TotalRowsAttempted = 10,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        failedResult.Errors.Add("Copy failed");
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(3612, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVORGBT_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 3613, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3613.csv" };
        _storage.Setup(s => s.DownloadFile("TVORGBT_3613.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvorgbt_stg",
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
        Assert.Equal("tvorgbt_stg", capturedConfig!.TableName);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TVORGBT_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 3630, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3630.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3630, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVORGBT_Success_Processed()
    {
        var load = new DataLoad { Id = 3631, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3631.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 3631, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3631, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVORGBT_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 3632, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3632.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(5, 3, 2, null, null, "sp_validoriginbillterm_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvorgbt_stg", "tvorgbt", 3632, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(3632, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateLoadReferenceAsync("tvorgbt_stg", "tvorgbt", 3632, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVORGBT_MergeError_AddsErrorRecord()
    {
        var load = new DataLoad { Id = 3633, LoadTableName = "tvorgbt", FileLocation = "gs://bucket/TVORGBT_3633.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_validoriginbillterm_merge_proc", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(3633, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
