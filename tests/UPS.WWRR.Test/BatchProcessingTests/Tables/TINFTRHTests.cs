#nullable enable
using Moq;
using UPS.WWRR.Business.Common.Enum;
using UPS.WWRR.Business.DTO.Models.Request;
using UPS.WWRR.Business.DTO.Models.Response;
using UPS.WWRR.Business.Repositories;
using UPS.WWRR.Data.Models;
using UPS.WWRR.UnitTests.BatchProcessing.Base;

namespace UPS.WWRR.UnitTests.BatchProcessing.Tables;

public class TINFTRHTests : BatchProcessorTests
{

    [Fact]
    public async Task ValidateLoadsAsync_TINFTRH_SetsReadyToProcess_OnSuccess()
    {
        var load = new DataLoad { Id = 151, LoadTableName = "tinftrh", FileLocation = "gs://bucket/TINFTRH_151.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.InformationalAccessorialThresholdDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(true, new List<string>()));
        _storage.Setup(s => s.DownloadFile("TINFTRH_151.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(151, LoadStatus.ReadyToProcess, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoadsAsync_TINFTRH_SetsFailedValidation_OnFailure()
    {
        var load = new DataLoad { Id = 152, LoadTableName = "tinftrh", FileLocation = "gs://bucket/TINFTRH_152.csv" };
        _validator.Setup(v => v.ValidateCsvAsync<UPS.WWRR.Business.DTO.Models.LoadTableDto.InformationalAccessorialThresholdDto>(It.IsAny<string>()))
            .ReturnsAsync(new CsvValidationResponse(false, new List<string> { "e" }));
        _storage.Setup(s => s.DownloadFile("TINFTRH_152.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "ValidateLoadsAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(152, LoadStatus.FailedValidation, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyBatchLoadAsync_TINFTRH_SetsProcessingOnStart()
    {
        var load = new DataLoad { Id = 511, LoadTableName = "tinftrh", FileLocation = "gs://bucket/TINFTRH_511.csv" };
        _storage.Setup(s => s.DownloadFile("TINFTRH_511.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _copy.Setup(c => c.CopyAsync(It.IsAny<string>(), It.IsAny<TableConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CopyBatchResultDto { TableName = "tinftrh_stg", SourceFile = "temp", RowsLoaded = 5, TotalRowsAttempted = 5, StartedAt = DateTimeOffset.UtcNow, CompletedAt = DateTimeOffset.UtcNow });
        var sut = CreateSut();
        var tempFiles = new Dictionary<string, string>();
        await InvokeAsync<object>(sut, "CopyBatchLoadAsync", new List<DataLoad> { load }, CancellationToken.None, tempFiles);
        _repo.Verify(r => r.UpdateStatusAsync(511, LoadStatus.Processing, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINFTRH_FailedMerge_UpdatesFailed()
    {
        var load = new DataLoad { Id = 530, LoadTableName = "tinftrh", FileLocation = "gs://bucket/TINFTRH_530.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(0, 0, 0, null, null, "sp", "", "error occurred"));
        _repo.Setup(r => r.AddErrorsAsync(It.IsAny<IEnumerable<DataLoadError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(530, LoadStatus.Failed, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PerformMergeLoadAsync_TINFTRH_Success_Processed()
    {
        var load = new DataLoad { Id = 531, LoadTableName = "tinftrh", FileLocation = "gs://bucket/TINFTRH_531.csv" };
        _repo.Setup(r => r.ExecuteMergeStoredProcedureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MergeResult(1, 2, 0, null, null, "sp", null, null));
        _repo.Setup(r => r.UpdateLoadReferenceAsync(It.IsAny<string>(), It.IsAny<string>(), 531, It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        var sut = CreateSut();
        await InvokeAsync<object>(sut, "PerformMergeLoadAsync", new List<DataLoad> { load }, CancellationToken.None);
        _repo.Verify(r => r.UpdateStatusAsync(531, LoadStatus.Processed, It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
