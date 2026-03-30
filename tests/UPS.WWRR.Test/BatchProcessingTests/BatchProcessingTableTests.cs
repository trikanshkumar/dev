#nullable enable
using AutoFixture;
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.LoadTableDto;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;
using Xunit.Abstractions;
using System.Linq;
using UPS.WWRR.Business.Common.Constants;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class BatchProcessingTableTests : BatchProcessorTests
{
    private readonly Fixture _fixture = new();

    public static readonly TheoryData<string> Tables =
    [
        "taltccy",
        "tasybrl",
        "tasytrh",
        "tauhist",
        "tbmavcs",
        "tbrchac",
        "tcnyasy",
        "tcoldec",
        "tcyblty",
        "tczmsys",
        "tdecode",
        "tdfwthr",
        "tdstsvp",
        "tfputrh",
        "tfscidx",
        "tfscmap",
        "timpsvc",
        "tinfchg",
        "tinfrat",
        "tinftrh",
        "tinscri",
        "tiraccy",
        "tlmtvlu",
        "tmincri",
        "tpslbur",
        "trastd",
        "tratrul",
        "tsdrwsf",
        "tsiarav",
        "tspmycd",
        "tsubchg",
        "tsvcacp",
        "tsvcdfl",
        "tsvcdgr",
        "tvasyln",
        "tvdstbt",
        "tvdsvcf",
        "tvlnsvc",
        "tvorgbt",
        "tvosvcf",
        "tvpaqmt",
        "tvsvcpk",
        "twgttrh"
    ];

    #region BuildLoadsAsync Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_ValidSingleLoad_Inserts(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), "2026_03_11_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Single(list);
        Assert.Equal(loadTableName.ToUpper(), list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_AlreadyExists_SkipsInsert(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), "2026_3_11_1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_MultipleLoads_InsertsAll(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var secondLoadFileName = $"{loadTableName.ToUpper()}_2026_03_11_2.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC\n{loadTableName.ToUpper()},{secondLoadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal(loadTableName.ToUpper(), l.LoadTableName));
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_GetFileAsStringThrowsException_AddsNoLoadsAndReturnsEmptyList(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ThrowsAsync(new Exception());
        
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);
        Assert.Empty(list);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_MalformedFilenameInReceiptFile_OnlyAddsAndReturnsLoadsWithValidFilename(string loadTableName)
    {
        // Arrange
        // get another table
        var tables = new List<string>(Tables);
        var otherTable = tables.First(t => t != loadTableName);

        var loadFileName = $"{loadTableName.ToUpper()}_1234.csv";
        var otherLoadFileName = $"{otherTable.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC\n{otherTable.ToUpper()},{otherLoadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        // Act
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert that only the valid table is returned
        Assert.Single(list);
        Assert.Equal(otherTable.ToUpper(), list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_DuplicateEntryInReceipt_OnlyAddsAndReturnsSingleLoad(string loadTableName)
    {
        // Arrange
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";

        // list the file twice in the receipt
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC\n{loadTableName.ToUpper()},{loadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), "2026_03_11_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        
        // Act
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert that it should only add and return a single load
        Assert.Single(list);
        Assert.Equal(loadTableName.ToUpper(), list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_MissingDestinationColumn_AddsAndReturnsLoadWithDefaultDestinationValue(string loadTableName)
    {
        // Arrange
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";

        // Missing Destination Column
        var receiptContent = $"TableName,FileExtractName\n{loadTableName.ToUpper()},{loadFileName}";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), "2026_03_11_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));
        var sut = CreateSut();

        // Act
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert the load for this table was returned with the default destination value
        Assert.Single(list);
        Assert.Equal(loadTableName.ToUpper(), list[0].LoadTableName);
        Assert.Equal(ServiceConstants.defaultDestinationValue, list[0].DataSource);

        // Assert that the load added to the database with the default destination value
        _repo.Verify(r => r.AddLoadsAsync(It.Is<IEnumerable<DataLoad>>(l => l.Count() == 1 && l.First().DataSource == ServiceConstants.defaultDestinationValue), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task BuildLoadsAsync_InvalidDateLoadIdValues_OnlyAddsAndReturnsLoadsWithValidFilename(string loadTableName)
    {
        // Arrange
        // get another table
        var tables = new List<string>(Tables);
        var otherTable = tables.First(t => t != loadTableName);

        var loadFileName = $"{loadTableName.ToUpper()}_1234.csv";
        var otherLoadFileName = $"{otherTable.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC\n{otherTable.ToUpper()},{otherLoadFileName},SRC";

        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        // Act
        var sut = CreateSut();
        var list = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        // Assert that only the valid table is returned
        Assert.Single(list);
        Assert.Equal(otherTable.ToUpper(), list[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }



    #endregion

    #region ValidateLoadsAsync Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_SetsReadyToProcess_OnSuccess(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_SetsFailedValidation_OnFailure(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "invalid" }));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "invalid" }));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_MultipleValidationErrors_RecordsAllErrors(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };

        var errors = _fixture.Create<List<string>>();

        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(false, errors));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.AddDetailAsync(It.IsAny<DataLoadDetail>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DataLoadDetail d, CancellationToken _) => { d.Id = 1; return d; });
        _repo.Setup(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.AddExceptionsAsync(It.IsAny<IEnumerable<DataLoadException>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }



    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_FileNotFoundInGcs_FailsValidation_AddsException_UpdatesFileLocationToEmpty(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };

        _storage.Setup(s => s.FileExistsAsync(fileName, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _storage.Setup(s => s.GetFilenameCaseInsensitive(fileName)).Returns((string?)null);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.fileNotFoundError
            ), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateFileLocation(loadId, string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        _storage.Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_FileCaseMismatch_FailsValidation_AddsExceptionWithSpecialStatus(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };

        _storage.Setup(s => s.FileExistsAsync(fileName, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // return the filename but in lowercase, which will be different as the table name in the original string is in uppercase
        _storage.Setup(s => s.GetFilenameCaseInsensitive(fileName)).Returns(fileName.ToLower());
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.filenameCaseMismatchError
            ), It.IsAny<CancellationToken>()), Times.Once);
        _storage.Verify(s => s.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_ValidationThrowsException_FailsValidation_AddsException(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ThrowsAsync(new Exception());
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception());
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.validateLoadsError
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CopyBatchLoadAsync Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_SetsProcessingOnStart(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = stagingTableName, SourceFile = "temp", RowsLoaded = 5, TotalRowsAttempted = 5, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_CopyFails_SetsFailedStatus(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failedResult = new CopyBatchResultDto
        {
            TableName = stagingTableName,
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
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_UsesCorrectStagingTable(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        TableConfigurationRequest? capturedConfig = null;
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, TableConfigurationRequest, CancellationToken>((_, cfg, _) => capturedConfig = cfg)
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
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
        Assert.Equal(stagingTableName, capturedConfig!.TableName);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_LargeFile_ProcessesSuccessfully(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
                SourceFile = "temp",
                RowsLoaded = 500000,
                TotalRowsAttempted = 500000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_PartialFailure_ReportsErrors(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var partialResult = new CopyBatchResultDto
        {
            TableName = stagingTableName,
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
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_ZeroRowsLoaded_SetsMissingDataStatus_AddsException(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
                SourceFile = "temp",
                RowsLoaded = 0,
                TotalRowsAttempted = 0,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.FailedMissingData, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.emptyDataFileError
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task CopyBatchLoadAsync_CopyBatchAsyncThrowsException_SetsFailedStatus_AddsException(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.copyBatchLoadError
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region PerformMergeLoadAsync Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_FailedMerge_UpdatesFailed(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp_example", "", "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_Success_Processed(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(2, 1, 0, null, null, "sp_example", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_WithInsertUpdateDelete_Success(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(100, 50, 25, null, null, "sp_example", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(175);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_MergeError_AddsErrorRecord(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23505", "ERROR", "sp_example", "42", "duplicate key value violates unique constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_LargeDataSet_Success(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(250000, 40000, 10000, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(300000);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_OnlyInserts_Success(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(500, 0, 0, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(500);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_OnlyUpdates_Success(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 200, 0, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_OnlyDeletes_Success(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 150, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_ForeignKeyViolation_Failed(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, "23503", "ERROR", "sp_example_merge_proc", "55", "insert or update on table violates foreign key constraint"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_MergeProcThrowsException_SetsFailedStatusAndAddsException(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Failed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddExceptionsAsync(It.Is<IEnumerable<DataLoadException>>(l =>
            l.Count() == 1 &&
            l.First().TableName.Equals(loadTableName, StringComparison.CurrentCultureIgnoreCase) &&
            l.First().ErrorFieldName == ServiceConstants.performMergeLoadError
        ), It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetDescriptor Tests
    [Theory]
    [MemberData(nameof(Tables))]
    public async Task GetDescriptor_ReturnsCorrectDescriptor(string loadTableName)
    {
        // Arrange
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var validatorInvocations = 0;

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .Callback(() => validatorInvocations++)
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .Callback(() => validatorInvocations++)
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();

        // Act
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        // Assert
        Assert.Equal(1, validatorInvocations);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task GetDescriptor_UppercaseTableName_Works(string loadTableName)
    {
        // Arrange
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var validatorInvocations = 0;

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName.ToUpper(), FileLocation = fileLocation };
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .Callback(() => validatorInvocations++)
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .Callback(() => validatorInvocations++)
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();

        // Act
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);

        // Assert
        Assert.Equal(1, validatorInvocations);
    }

    #endregion

    #region End to End Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task EndToEnd_FullSuccessFlow(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";

        // Build
        _storage.Setup(s => s.DiscoverReceiptLogFileAsync(It.IsAny<CancellationToken>())).ReturnsAsync("receipt.csv");
        _storage.Setup(s => s.GetFileAsString("receipt.csv")).ReturnsAsync(receiptContent);
        _repo.Setup(r => r.ExistsAsync(loadTableName.ToUpper(), loadFileName, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()))
            .Returns<IEnumerable<DataLoad>, CancellationToken>((loads, _) => Task.FromResult(loads.ToList()));

        var sut = CreateSut();
        var buildResult = await InvokeAsync<List<DataLoad>>(sut, "BuildLoadsAsync", CancellationToken.None);

        Assert.Single(buildResult);
        Assert.Equal(loadTableName.ToUpper(), buildResult[0].LoadTableName);
        _repo.Verify(r => r.AddLoadsAsync(It.IsAny<IEnumerable<DataLoad>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task EndToEnd_ValidationThenCopy_Success(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{loadFileName}";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(loadFileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
                SourceFile = "temp",
                RowsLoaded = 1000,
                TotalRowsAttempted = 1000,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });

        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task EndToEnd_FullPipeline_Success(string loadTableName)
    {
        var loadFileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{loadFileName}";
        var receiptContent = $"TableName,FileExtractName,Destination\n{loadTableName.ToUpper()},{loadFileName},SRC";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };

        // Validation
        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(loadFileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();

        // Validate
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);

        // Copy
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto
            {
                TableName = stagingTableName,
                SourceFile = "temp",
                RowsLoaded = 100,
                TotalRowsAttempted = 100,
                StartedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            });
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);

        // Merge
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(80, 15, 5, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion


    #region Composite Key Validation Tests

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task ValidateLoadsAsync_DuplicateCompositeKey_Success(string loadTableName)
    {
        // This test verifies validation passes since duplicate key handling is done at the merge level
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName.ToUpper(), FileLocation = fileLocation };

        _validator.Setup(v => v.ValidateCsvAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _validator.Setup(v => v.ValidateCsvChunkedAsync<It.IsAnyType>(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile(fileName, It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_DuplicateCompositeKey_HandlesUpdate(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName.ToUpper(), FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 10, 0, null, null, "sp_example_merge_proc", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

}
