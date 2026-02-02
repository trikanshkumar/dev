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
/// Unit tests for TVASYLN (Valid Accessorial Lane) table batch processing.
/// </summary>
public class TVASYLNTests : BatchProcessorTests
{
    #region BuildLoadsAsync Tests

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_NoReceipt_ReturnsEmpty()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_EmptyReceipt_ReturnsEmpty()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(string.Empty);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_InvalidColumns_ReturnsEmpty()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync("Wrong,Header\nval1,val2");
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_ValidSingleLoad_Inserts()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVASYLN_58001.csv,SRC";
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVASYLN", 58001, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal("TVASYLN", list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_AlreadyExists_SkipsInsert()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVASYLN_58002.csv,SRC";
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVASYLN", 58002, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TVASYLN_MultipleLoads_InsertsAll()
    {
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVASYLN_58003.csv,SRC\nTVASYLN_58004.csv,SRC";
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVASYLN", It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal("TVASYLN", l.LoadTableName));
    }

    #endregion

    #region ValidateLoadsAsync Tests

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 5801, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5801.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5801.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5801, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 5802, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5802.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid origin country code" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5802.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5802, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_MultipleValidationErrors_RecordsAllErrors()
    {
        var load = new DataLoad { Id = 5803, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5803.csv" };
        var errors = new List<string>
        {
            "Line 2: Invalid ORG_CNY_CD - exceeds max length",
            "Line 3: Invalid DTN_CNY_CD - exceeds max length",
            "Line 5: Invalid ASY_SVC_TYP_CD - exceeds max length",
            "Line 8: Invalid REC_EFF_STT_DT - invalid date format",
            "Line 10: Invalid REC_EFF_END_DT - invalid date format"
        };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5803.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(5803, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidOriginCountryCode_Fails()
    {
        var load = new DataLoad { Id = 5804, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5804.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: ORG_CNY_CD exceeds maximum length of 2" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5804.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5804, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidDestinationCountryCode_Fails()
    {
        var load = new DataLoad { Id = 5805, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5805.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: DTN_CNY_CD exceeds maximum length of 2" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5805.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5805, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidAccessorialServiceTypeCode_Fails()
    {
        var load = new DataLoad { Id = 5806, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5806.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: ASY_SVC_TYP_CD exceeds maximum length of 3" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5806.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5806, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidServiceTypeCode_Fails()
    {
        var load = new DataLoad { Id = 5807, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5807.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: SVC_TYP_CD exceeds maximum length of 3" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5807.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5807, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidMovementDirectionCode_Fails()
    {
        var load = new DataLoad { Id = 5808, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5808.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: MVM_DRC_CD exceeds maximum length of 1" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5808.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5808, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidApprovalStatusCode_Fails()
    {
        var load = new DataLoad { Id = 5809, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5809.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: APV_STS_CD exceeds maximum length of 2" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5809.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5809, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_EmptyFile_SetsReadyToProcess()
    {
        var load = new DataLoad { Id = 5810, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5810.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5810.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5810, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TVASYLN_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 5811, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5811.csv" };
		_storage.Setup(s => s.DownloadFile("TVASYLN_5811.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvasyln_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5811, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVASYLN_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 5812, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5812.csv" };
		_storage.Setup(s => s.DownloadFile("TVASYLN_5812.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = "tvasyln_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(5812, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVASYLN_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 5813, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5813.csv" };
		_storage.Setup(s => s.DownloadFile("TVASYLN_5813.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvasyln_stg",
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
        Assert.Equal("tvasyln_stg", capturedConfig!.TableName);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVASYLN_LargeFile_ProcessesSuccessfully()
    {
        var load = new DataLoad { Id = 5814, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5814.csv" };
		_storage.Setup(s => s.DownloadFile("TVASYLN_5814.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvasyln_stg",
                SourceFile = "temp",
                RowsLoaded = 500000,
                TotalRowsAttempted = 500000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5814, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TVASYLN_PartialFailure_ReportsErrors()
    {
        var load = new DataLoad { Id = 5815, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5815.csv" };
		_storage.Setup(s => s.DownloadFile("TVASYLN_5815.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var partialResult = new CopyBatchResultDto
        {
            TableName = "tvasyln_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(5815, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 5830, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5830.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5830, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_Success_Processed()
    {
        var load = new DataLoad { Id = 5831, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5831.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 5831, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5831, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 5832, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5832.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 25, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5832, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(175);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5832, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5832, It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_MergeError_AddsErrorRecord()
    {
        var load = new DataLoad { Id = 5833, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5833.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_validaccessoriallane_merge_proc", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(5833, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_LargeDataSet_Success()
    {
        var load = new DataLoad { Id = 5834, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5834.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(250000, 40000, 10000, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5834, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(300000);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5834, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_OnlyInserts_Success()
    {
        var load = new DataLoad { Id = 5835, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5835.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(500, 0, 0, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5835, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(500);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5835, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_OnlyUpdates_Success()
    {
        var load = new DataLoad { Id = 5836, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5836.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 200, 0, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5836, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5836, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_OnlyDeletes_Success()
    {
        var load = new DataLoad { Id = 5837, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5837.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 150, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5837, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5837, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TVASYLN_ForeignKeyViolation_Failed()
    {
        var load = new DataLoad { Id = 5838, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5838.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23503", "ERROR", "sp_validaccessoriallane_merge_proc", "55", "insert or update on table violates foreign key constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(5838, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Fact]
    public async Task GetDescriptor_TVASYLN_ReturnsCorrectDescriptor()
    {
        var load = new DataLoad { Id = 5850, LoadTableName = "TVASYLN", FileLocation = "gs://bucket/TVASYLN_5850.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5850.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _validator.Verify(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetDescriptor_TVASYLN_LowercaseTableName_Works()
    {
        var load = new DataLoad { Id = 5851, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5851.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5851.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5851, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region End-to-End Flow Tests

    [Fact]
    public async Task EndToEnd_TVASYLN_FullSuccessFlow()
    {
        // Build
		_storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "FileExtractName,Source\nTVASYLN_5860.csv,SRC";
		_storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TVASYLN", 5860, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();
        var buildResult = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        Assert.Single(buildResult);
        Assert.Equal("TVASYLN", buildResult[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TVASYLN_ValidationThenCopy_Success()
    {
        var load = new DataLoad { Id = 5861, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5861.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5861.csv", It.IsAny<string>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5861, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvasyln_stg",
                SourceFile = "temp",
                RowsLoaded = 1000,
                TotalRowsAttempted = 1000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5861, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TVASYLN_FullPipeline_Success()
    {
        var load = new DataLoad { Id = 5862, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5862.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5862.csv", It.IsAny<string>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();

        // Validate
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5862, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tvasyln_stg",
                SourceFile = "temp",
                RowsLoaded = 100,
                TotalRowsAttempted = 100,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5862, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);

        // Merge
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(80, 15, 5, null, null, "sp_validaccessoriallane_merge_proc", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync("tvasyln_stg", "tvasyln", 5862, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(5862, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Specific Column Validation Tests

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidLaneClassTypeCode_Fails()
    {
        var load = new DataLoad { Id = 5870, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5870.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_UNT_PIR_CSF_CD exceeds maximum length of 1" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5870.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5870, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidAccessorialAlternateNumericCode_Fails()
    {
        var load = new DataLoad { Id = 5871, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5871.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: ASY_SVC_ALT_NMC_CD exceeds maximum length of 3" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5871.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5871, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidServiceTypeAlternateNumericCode_Fails()
    {
        var load = new DataLoad { Id = 5872, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5872.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: SVC_TYP_ALT_NMC_CD exceeds maximum length of 2" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5872.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5872, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidOriginGeopoliticalCountry_Fails()
    {
        var load = new DataLoad { Id = 5873, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5873.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: ORG_GPN_MNM_TE exceeds maximum length of 4" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5873.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5873, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidDestinationGeopoliticalCountry_Fails()
    {
        var load = new DataLoad { Id = 5874, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5874.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: DTN_GPN_MNM_TE exceeds maximum length of 4" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5874.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5874, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidEffectiveStartDate_Fails()
    {
        var load = new DataLoad { Id = 5875, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5875.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: REC_EFF_STT_DT is not a valid date" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5875.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5875, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TVASYLN_InvalidEffectiveEndDate_Fails()
    {
        var load = new DataLoad { Id = 5876, LoadTableName = "tvasyln", FileLocation = "gs://bucket/TVASYLN_5876.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ValidAccessorialLaneDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: REC_EFF_END_DT is not a valid date" }));
		_storage.Setup(s => s.DownloadFile("TVASYLN_5876.csv", It.IsAny<string>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(5876, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
