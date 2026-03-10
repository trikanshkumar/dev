#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class TINFCHGTests : BatchProcessorTests
{

    [Fact]
    public async Task ValidateLoadsAsync_TINFCHG_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 1101, LoadTableName = "tinfchg", FileLocation = "gs://bucket/TINFCHG_1101.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.InformationalAccessorialChargeDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINFCHG_1101.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(1101, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINFCHG_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 1102, LoadTableName = "tinfchg", FileLocation = "gs://bucket/TINFCHG_1102.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.InformationalAccessorialChargeDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "invalid" }));
        _storage.Setup(s => s.DownloadFile("TINFCHG_1102.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(1102, LoadStatus.FailedValidation, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINFCHG_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 1111, LoadTableName = "tinfchg", FileLocation = "gs://bucket/TINFCHG_1111.csv" };
        _storage.Setup(s => s.DownloadFile("TINFCHG_1111.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinfchg_stg", SourceFile = "temp", RowsLoaded = 9, TotalRowsAttempted = 9, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(1111, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINFCHG_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 1130, LoadTableName = "tinfchg", FileLocation = "gs://bucket/TINFCHG_1130.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", string.Empty, "merge failed"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(1130, LoadStatus.Failed, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINFCHG_Success_Processed()
    {
        var load = new DataLoad { Id = 1131, LoadTableName = "tinfchg", FileLocation = "gs://bucket/TINFCHG_1131.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(4, 1, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.MarkStagingCompletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(1131, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
