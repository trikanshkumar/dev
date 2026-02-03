#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class TDFWTHRTests : BatchProcessorTests
{

    [Fact]
    public async Task ValidateLoadsAsync_TDFWTHR_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 601, LoadTableName = "tdfwthr", FileLocation = "gs://bucket/TDFWTHR_601.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.DeficitWeightThresholdDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TDFWTHR_601.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(601, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TDFWTHR_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 602, LoadTableName = "tdfwthr", FileLocation = "gs://bucket/TDFWTHR_602.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.DeficitWeightThresholdDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "error" }));
        _storage.Setup(s => s.DownloadFile("TDFWTHR_602.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(602, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TDFWTHR_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 611, LoadTableName = "tdfwthr", FileLocation = "gs://bucket/TDFWTHR_611.csv" };
        _storage.Setup(s => s.DownloadFile("TDFWTHR_611.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tdfwthr_stg", SourceFile = "temp", RowsLoaded = 10, TotalRowsAttempted = 10, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(611, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TDFWTHR_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 630, LoadTableName = "tdfwthr", FileLocation = "gs://bucket/TDFWTHR_630.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "error occurred"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(630, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TDFWTHR_Success_Processed()
    {
        var load = new DataLoad { Id = 631, LoadTableName = "tdfwthr", FileLocation = "gs://bucket/TDFWTHR_631.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(2, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 631, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(631, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
