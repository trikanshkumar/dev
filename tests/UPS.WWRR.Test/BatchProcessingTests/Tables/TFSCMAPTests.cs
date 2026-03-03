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
/// Unit tests for TFSCMAP (Fuel Surcharge Category Map) table batch processing.
/// </summary>
public class TFSCMAPTests : BatchProcessorTests
{
    #region BuildLoadsAsync Tests

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_NoReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_EmptyReceipt_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(string.Empty);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_InvalidColumns_ReturnsEmpty()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync("Wrong,Header\nval1,val2");
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_ValidSingleLoad_Inserts()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTFSCMAP,TFSCMAP_2026_02_26_1.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TFSCMAP", "2026_02_26_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal("TFSCMAP", list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_AlreadyExists_SkipsInsert()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTFSCMAP,TFSCMAP_58002.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TFSCMAP", "2026_02_26_1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BuildLoadsAsync_TFSCMAP_MultipleLoads_InsertsAll()
    {
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTFSCMAP,TFSCMAP_2026_02_26_3.csv,SRC\nTFSCMAP,TFSCMAP_2026_02_26_4.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TFSCMAP", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal("TFSCMAP", l.LoadTableName));
    }

    #endregion

    #region ValidateLoadsAsync Tests

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 6601, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6601.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6601.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6601, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 6602, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6602.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Invalid export country code" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6602.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6602, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_MultipleValidationErrors_RecordsAllErrors()
    {
        var load = new DataLoad { Id = 6603, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6603.csv" };
        var errors = new List<string>
        {
            "Line 2: Invalid GPN_XPT_CNY_CD - exceeds max length",
            "Line 3: Invalid GPN_IPT_CNY_CD - exceeds max length",
            "Line 5: Invalid MVM_DRC_CD - exceeds max length",
            "Line 8: Invalid REC_EFF_STT_DT - invalid date format",
            "Line 10: Invalid REC_EFF_END_DT - invalid date format"
        };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6603.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(6603, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidExportCountryCode_Fails()
    {
        var load = new DataLoad { Id = 6604, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6604.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_XPT_CNY_CD exceeds maximum length of 4" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6604.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6604, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidImportCountryCode_Fails()
    {
        var load = new DataLoad { Id = 6605, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6605.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: GPN_IPT_CNY_CD exceeds maximum length of 4" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6605.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6605, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidMovementDirectionCode_Fails()
    {
        var load = new DataLoad { Id = 6606, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6606.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: MVM_DRC_CD exceeds maximum length of 1" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6606.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6606, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidServiceTypeCode_Fails()
    {
        var load = new DataLoad { Id = 6607, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6607.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: SVC_TYP_CD exceeds maximum length of 3" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6607.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6607, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidCustomerClassificationTypeCode_Fails()
    {
        var load = new DataLoad { Id = 6608, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6608.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: CUS_CSF_TYP_CD exceeds maximum length of 2" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6608.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6608, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidCurrencyCode_Fails()
    {
        var load = new DataLoad { Id = 6609, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6609.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: CCY_CD exceeds maximum length of 3" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6609.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6609, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidApprovalStatusCode_Fails()
    {
        var load = new DataLoad { Id = 6610, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6610.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: APV_STS_CD exceeds maximum length of 2" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6610.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6610, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidIndexFuelCategoryCode_Fails()
    {
        var load = new DataLoad { Id = 6611, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6611.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: PSE_IDX_FU_CGY_CD exceeds maximum length of 2" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6611.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6611, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidEffectiveStartDate_Fails()
    {
        var load = new DataLoad { Id = 6612, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6612.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: REC_EFF_STT_DT is not a valid date" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6612.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6612, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidEffectiveEndDate_Fails()
    {
        var load = new DataLoad { Id = 6613, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6613.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: REC_EFF_END_DT is not a valid date" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6613.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6613, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_InvalidRecordInsertTimestamp_Fails()
    {
        var load = new DataLoad { Id = 6614, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6614.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "Line 2: REC_INS_TS is not a valid timestamp" }));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6614.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6614, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_EmptyFile_SetsReadyToProcess()
    {
        var load = new DataLoad { Id = 6615, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6615.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6615.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6615, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Fact]
    public async Task CopyBatchLoadAsync_TFSCMAP_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 6620, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6620.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6620.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tfscmap_stg",
                SourceFile = "temp",
                RowsLoaded = 10,
                TotalRowsAttempted = 10,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6620, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TFSCMAP_CopyFails_SetsFailedStatus()
    {
        var load = new DataLoad { Id = 6621, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6621.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6621.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = "tfscmap_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(6621, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TFSCMAP_UsesCorrectStagingTable()
    {
        var load = new DataLoad { Id = 6622, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6622.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6622.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tfscmap_stg",
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
        Assert.Equal("tfscmap_stg", capturedConfig!.TableName);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TFSCMAP_LargeFile_ProcessesSuccessfully()
    {
        var load = new DataLoad { Id = 6623, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6623.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6623.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tfscmap_stg",
                SourceFile = "temp",
                RowsLoaded = 500000,
                TotalRowsAttempted = 500000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6623, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TFSCMAP_PartialFailure_ReportsErrors()
    {
        var load = new DataLoad { Id = 6624, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6624.csv" };
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6624.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var partialResult = new CopyBatchResultDto
        {
            TableName = "tfscmap_stg",
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
        _repo.Verify(r => r.UpdateStatusAsync(6624, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 6630, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6630.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6630, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_Success_Processed()
    {
        var load = new DataLoad { Id = 6631, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6631.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(8, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(9);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6631, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_WithInsertUpdateDelete_Success()
    {
        var load = new DataLoad { Id = 6632, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6632.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 25, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(175);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6632, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_MergeError_AddsErrorRecord()
    {
        var load = new DataLoad { Id = 6633, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6633.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_fuelsurchargecategorymap_merge_proc", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(6633, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_LargeDataSet_Success()
    {
        var load = new DataLoad { Id = 6634, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6634.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(250000, 40000, 10000, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(300000);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6634, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_OnlyInserts_Success()
    {
        var load = new DataLoad { Id = 6635, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6635.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(500, 0, 0, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(500);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6635, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_OnlyUpdates_Success()
    {
        var load = new DataLoad { Id = 6636, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6636.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 200, 0, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6636, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_OnlyDeletes_Success()
    {
        var load = new DataLoad { Id = 6637, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6637.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 150, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6637, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_ForeignKeyViolation_Failed()
    {
        var load = new DataLoad { Id = 6638, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6638.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23503", "ERROR", "sp_fuelsurchargecategorymap_merge_proc", "55", "insert or update on table violates foreign key constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(6638, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDescriptor Tests

    [Fact]
    public async Task GetDescriptor_TFSCMAP_ReturnsCorrectDescriptor()
    {
        var load = new DataLoad { Id = 6650, LoadTableName = "TFSCMAP", FileLocation = "gs://bucket/TFSCMAP_6650.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6650.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _validator.Verify(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetDescriptor_TFSCMAP_LowercaseTableName_Works()
    {
        var load = new DataLoad { Id = 6651, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6651.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6651.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6651, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region End-to-End Flow Tests

    [Fact]
    public async Task EndToEnd_TFSCMAP_FullSuccessFlow()
    {
        // Build
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        var content = "TableName,FileExtractName,Destination\nTFSCMAP,TFSCMAP_2026_02_26_1.csv,SRC";
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(content);
        _repo.Setup(r => r.ExistsAsync("TFSCMAP", "2026_02_26_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();
        var buildResult = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        Assert.Single(buildResult);
        Assert.Equal("TFSCMAP", buildResult[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TFSCMAP_ValidationThenCopy_Success()
    {
        var load = new DataLoad { Id = 6661, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6661.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6661.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6661, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tfscmap_stg",
                SourceFile = "temp",
                RowsLoaded = 1000,
                TotalRowsAttempted = 1000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6661, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndToEnd_TFSCMAP_FullPipeline_Success()
    {
        var load = new DataLoad { Id = 6662, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6662.csv" };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6662.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();

        // Validate
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6662, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = "tfscmap_stg",
                SourceFile = "temp",
                RowsLoaded = 100,
                TotalRowsAttempted = 100,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6662, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);

        // Merge
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(80, 15, 5, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6662, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Composite Key Validation Tests

    [Fact]
    public async Task ValidateLoadsAsync_TFSCMAP_DuplicateCompositeKey_Success()
    {
        // This test verifies validation passes since duplicate key handling is done at the merge level
        var load = new DataLoad { Id = 6670, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6670.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<FuelSurchargeCategoryMapDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TFSCMAP_6670.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6670, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TFSCMAP_DuplicateCompositeKey_HandlesUpdate()
    {
        var load = new DataLoad { Id = 6671, LoadTableName = "tfscmap", FileLocation = "gs://bucket/TFSCMAP_6671.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 10, 0, null, null, "sp_fuelsurchargecategorymap_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6671, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
