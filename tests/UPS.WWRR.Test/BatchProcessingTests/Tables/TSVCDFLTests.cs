#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.LoadTableDto;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;
using Xunit;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class TSVCDFLTests : BatchProcessorTests
{
    [Fact]
    public async Task ValidateLoadsAsync_TSVCDFL_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 6401, LoadTableName = "tsvcdfl", FileLocation = "gs://bucket/TSVCDFL_6401.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ServiceDefaultRulesDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TSVCDFL_6401.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6401, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TSVCDFL_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 6402, LoadTableName = "tsvcdfl", FileLocation = "gs://bucket/TSVCDFL_6402.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<ServiceDefaultRulesDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "invalid" }));
        _storage.Setup(s => s.DownloadFile("TSVCDFL_6402.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6402, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TSVCDFL_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 6411, LoadTableName = "tsvcdfl", FileLocation = "gs://bucket/TSVCDFL_6411.csv" };
        _storage.Setup(s => s.DownloadFile("TSVCDFL_6411.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tsvcdfl_stg", SourceFile = "temp", RowsLoaded = 9, TotalRowsAttempted = 9, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(6411, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TSVCDFL_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 6430, LoadTableName = "tsvcdfl", FileLocation = "gs://bucket/TSVCDFL_6430.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6430, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TSVCDFL_Success_Processed()
    {
        var load = new DataLoad { Id = 6431, LoadTableName = "tsvcdfl", FileLocation = "gs://bucket/TSVCDFL_6431.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(4, 2, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(6431, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
