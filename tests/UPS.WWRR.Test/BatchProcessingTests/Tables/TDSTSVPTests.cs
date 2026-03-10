#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class TDSTSVPTests : BatchProcessorTests
{

    [Fact]
    public async Task ValidateLoadsAsync_TDSTSVP_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 103, LoadTableName = "tdstsvp", FileLocation = "gs://bucket/TDSTSVP_103.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.DestinationZipSvcAsyValidationDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TDSTSVP_103.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(103, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TDSTSVP_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 104, LoadTableName = "tdstsvp", FileLocation = "gs://bucket/TDSTSVP_104.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.DestinationZipSvcAsyValidationDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "bad" }));
        _storage.Setup(s => s.DownloadFile("TDSTSVP_104.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(104, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TDSTSVP_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 113, LoadTableName = "tdstsvp", FileLocation = "gs://bucket/TDSTSVP_113.csv" };
        _storage.Setup(s => s.DownloadFile("TDSTSVP_113.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tdstsvp_stg", SourceFile = "temp", RowsLoaded = 7, TotalRowsAttempted = 7, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(113, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TDSTSVP_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 130, LoadTableName = "tdstsvp", FileLocation = "gs://bucket/TDSTSVP_130.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", "", "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(130, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TDSTSVP_Success_Processed()
    {
        var load = new DataLoad { Id = 131, LoadTableName = "tdstsvp", FileLocation = "gs://bucket/TDSTSVP_131.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(3, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(131, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
