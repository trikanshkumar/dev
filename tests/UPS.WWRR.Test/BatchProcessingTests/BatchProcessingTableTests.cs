#nullable enable
using AutoFixture;
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class BatchProcessingTableTests : BatchProcessorTests
{
    private readonly Fixture _fixture = new();

    public static TheoryData<string> Tables =
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
    public async Task CopyBatchLoadAsync_SetsProcessingOnStart(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var stagingTableName = $"{loadTableName}_stg";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _storage.Setup(s => s.DownloadFile("TASYBRL_111.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = stagingTableName, SourceFile = "temp", RowsLoaded = 5, TotalRowsAttempted = 5, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Tables))]
    public async Task PerformMergeLoadAsync_FailedMerge_UpdatesFailed(string loadTableName)
    {
        var fileName = $"{loadTableName.ToUpper()}_2026_03_11_1.csv";
        var fileLocation = $"gs://bucket/{fileName}";
        var loadId = _fixture.Create<int>();

        var load = new DataLoad { Id = loadId, LoadTableName = loadTableName, FileLocation = fileLocation };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", "", "merge failed"));
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
            .ReturnsAsync(new MergeResult(2, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(loadId, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
